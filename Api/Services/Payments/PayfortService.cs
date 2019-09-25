using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Common.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PayfortService : IPayfortService
    {
        public PayfortService(ILogger<PayfortService> logger, IHttpClientFactory clientFactory, IOptions<PayfortOptions> options)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _options = options;
        }

        public async Task<Result<TokenizationInfo>> Tokenize(TokenizationRequest request)
        {
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    var requestContent = GetSignedContent();

                    using (var response = await client.PostAsync(Options.TokenizationUrl, requestContent))
                    {
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<TokenizationInfo>($"Payfort unsuccessfull status: {response.StatusCode}. message: {await response.Content.ReadAsStringAsync()}");
                        var query = HttpUtility.ParseQueryString(response.RequestMessage.RequestUri.Query);
                        var model = Parse<PayfortTokenizationResponse>(query);

                        var signature = GetSignatureFromQuery(query, Options.ShaResponsePhrase, false);

                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Tokenization error: Invalid response signature. content: {0}", response.RequestMessage.RequestUri.Query);
                            return Result.Fail<TokenizationInfo>($"Tokenization error: Invalid response signature");
                        }

                        if (IsFailed(model))
                            return Result.Fail<TokenizationInfo>($"Tokenization error. {model.Status}: {model.ResponseMessage}");

                        return Result.Ok(new TokenizationInfo(model.ExpirationDate, model.CardNumber, model.TokenName, model.CardHolderName));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogPayfortClientException(ex);
                return Result.Fail<TokenizationInfo>(ex.Message);
            }

            HttpContent GetSignedContent()
            {
                var tokenRequest = new PayfortTokenizationRequest(
                    language: request.Language,
                    serviceCommand: "TOKENIZATION",
                    accessCode: Options.AccessCode,
                    cardNumber: request.CardNumber,
                    expiryDate: request.ExpirationDate,
                    merchantIdentifier: Options.Identifier,
                    merchantReference: request.ReferenceCode,
                    rememberMe: ToPayfortBoolean(request.RememberMe),
                    returnUrl: Options.ReturnUrl,
                    cardHolderName: request.CardHolderName,
                    cardSecurityCode: request.CardSecurityCode);
                tokenRequest.Signature = GetSignature(tokenRequest, Options.ShaRequestPhrase, true);
                var dict = GetDictFromObject(tokenRequest);
                var requestContent = new FormUrlEncodedContent(dict);

                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                return requestContent;
            }
            bool IsFailed(PayfortTokenizationResponse model) => model.ResponseCode != PayfortConst.TokenizationSuccessResponseCode;
        }

        public async Task<Result<CreditCardPaymentResult>> Pay(CreditCardPaymentRequest request)
        {
            // TODO: Refactor after tests in test eviroment
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    var paymentRequest = new PayfortPaymentRequest()
                    {
                        AccessCode = Options.AccessCode,
                        MerchantIdentifier = Options.Identifier,
                        MerchantReference = request.ReferenceCode,
                        Amount = ToPayfortAmount(request.Amount, request.Currency),
                        CardSecurityCode = request.CardSecurityCode,
                        Command = "PURCHASE",
                        Currency = request.Currency.ToString(),
                        CustomerName = request.CustomerName,
                        CustomerEmail = request.CustomerEmail,
                        CustomerIp = request.CustomerIp,
                        Language = request.Language,
                        RememberMe = ToPayfortBoolean(request.IsMemorable),
                        ReturnUrl = Options.ReturnUrl,
                        SettlementReference = request.ReferenceCode,
                        TokenName = request.TokenName
                    };
                    paymentRequest.Signature = GetSignature(paymentRequest, Options.ShaRequestPhrase, true);
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(paymentRequest, Settings), Encoding.UTF8, "application/json");
                    using (var response = await client.PostAsync(Options.PaymentUrl, jsonContent))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<CreditCardPaymentResult>(content);
                        var model = JsonConvert.DeserializeObject<PayfortPaymentResponse>(content, Settings);

                        if (model == null)
                            return Result.Fail<CreditCardPaymentResult>($"Invalid payfort payment response: {content}");

                        var signature = GetSignature(model, Options.ShaResponsePhrase, true);
                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Payment error: Invalid response signature. content: {0}", content);
                            return Result.Fail<CreditCardPaymentResult>($"Payment error: Invalid response signature");
                        }
                        
                        if (IsFailed(model))
                            return Result.Fail<CreditCardPaymentResult>($"Payment error. {model.Status}: {model.ResponseMessage}");

                        return Result.Ok(new CreditCardPaymentResult(model.Secure3D, model.SettlementReference, model.AuthorizationCode, model.FortId,
                            model.ExpirationDate, model.CardNumber, model.CardHolderName));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogPayfortClientException(ex);
                return Result.Fail<CreditCardPaymentResult>(ex.Message);
            }
            
            // TODO: Should be refactored after getting test environment. not success or 3dSecure
            bool IsFailed(PayfortPaymentResponse model) => model.ResponseCode != "00000";
        }

        private static string ToPayfortBoolean(bool value) => 
            value ? "YES" : "NO";

        private static decimal ToPayfortAmount(decimal amount, Currencies currency) => 
            amount * PaymentConstants.Multipliers[currency];

        private static Dictionary<string, string> GetDictFromObject<T>(T model)
        {
            var jObject = JObject.FromObject(model, JsonSerializer.Create(Settings));
            var dict = jObject.Properties().ToDictionary(p => p.Name, p => p.Value.Value<object>()?.ToString());
            return dict;
        }

        private static T Parse<T>(NameValueCollection collection)
        {
            var dict = collection.AllKeys.ToDictionary(k => k, k => collection.GetValues(k)?.FirstOrDefault());
            var json = JsonConvert.SerializeObject(dict, Settings);
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        private static string GetSignatureFromDict(Dictionary<string, string> dict, string pass, bool isRequest)
        {
            var filteredValues = dict
                .Where(kv => kv.Key != "signature" && // Do not include signature in all cases
                    (!isRequest || !IgnoredForSignature.Contains(kv.Key)) && // do not include some fields in request
                    kv.Value != null)
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}");
            var str = $"{pass}{string.Join("", filteredValues)}{pass}";
            using (var sha = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }

        private static string GetSignature<T>(T model, string pass, bool ignoreFileds)
        {
            var values = GetDictFromObject(model);
            return GetSignatureFromDict(values, pass, ignoreFileds);
        }

        public static string GetSignatureFromQuery(NameValueCollection query, string pass, bool ignoreFileds)
        {
            var dict = query.AllKeys.ToDictionary(k => k, k => query.GetValues(k)?.FirstOrDefault());
            return GetSignatureFromDict(dict, pass, ignoreFileds);
        }

        private readonly ILogger<PayfortService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IOptions<PayfortOptions> _options;
        private PayfortOptions Options => _options.Value;

        private static readonly string[] IgnoredForSignature = new[]
        {
            "card_security_code",
            "card_number",
            "expiry_date",
            "card_holder_name",
            "remember_me",
            "token_name"
        };

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
    }
}
