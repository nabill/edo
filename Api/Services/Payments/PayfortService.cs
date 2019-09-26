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

                        var signature = GetSignatureFromQuery(query, Options.ShaResponsePhrase);

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
                    accessCode: Options.AccessCode,
                    cardNumber: request.CardNumber,
                    expiryDate: request.ExpirationDate,
                    merchantIdentifier: Options.Identifier,
                    merchantReference: request.ReferenceCode,
                    rememberMe: ToPayfortBoolean(request.RememberMe),
                    returnUrl: Options.ReturnUrl,
                    cardHolderName: request.CardHolderName,
                    cardSecurityCode: request.CardSecurityCode);
                tokenRequest.Signature = GetSignatureFromObject(tokenRequest, Options.ShaRequestPhrase, IgnoredForTokenizationFields);
                var dict = GetDictFromObject(tokenRequest);
                var requestContent = new FormUrlEncodedContent(dict);

                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                return requestContent;
            }
            bool IsFailed(PayfortTokenizationResponse model) =>
                model.ResponseCode != PayfortConst.TokenizationSuccessResponseCode;
        }

        public async Task<Result<CreditCardPaymentResult>> Pay(CreditCardPaymentRequest request)
        {
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    var requestContent = GetSignedContent();
        
                    using (var response = await client.PostAsync(Options.PaymentUrl, requestContent))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<CreditCardPaymentResult>(content);
                        var model = JsonConvert.DeserializeObject<PayfortPaymentResponse>(content, Settings);

                        if (model == null)
                            return Result.Fail<CreditCardPaymentResult>($"Invalid payfort payment response: {content}");

                        var signature = GetSignatureFromJson(content, Options.ShaResponsePhrase);
                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Payment error: Invalid response signature. content: {0}", content);
                            return Result.Fail<CreditCardPaymentResult>($"Payment error: Invalid response signature");
                        }

                        var (_, isFailed, status, error) = GetStatus(model);
                        if (isFailed)
                            return Result.Fail<CreditCardPaymentResult>(error);

                        return Result.Ok(new CreditCardPaymentResult(model.Secure3d, model.SettlementReference, model.AuthorizationCode, model.FortId,
                            model.ExpirationDate, model.CardNumber, model.CardHolderName, status));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogPayfortClientException(ex);
                return Result.Fail<CreditCardPaymentResult>(ex.Message);
            }
            HttpContent GetSignedContent()
            {
                var paymentRequest = new PayfortPaymentRequest(
                    accessCode: Options.AccessCode,
                    merchantIdentifier: Options.Identifier,
                    merchantReference: request.ReferenceCode,
                    amount: ToPayfortAmount(request.Amount, request.Currency),
                    currency: request.Currency.ToString(),
                    customerName: request.CustomerName,
                    customerEmail: request.CustomerEmail,
                    customerIp: request.CustomerIp,
                    language: request.LanguageCode,
                    rememberMe: ToPayfortBoolean(request.IsOneTime),
                    returnUrl: Options.ReturnUrl,
                    settlementReference: request.ReferenceCode,
                    tokenName: request.Token,
                    // There are error "Invalid extra parameters" if secureCode filled for One time token
                    cardSecurityCode: request.IsOneTime ? null : request.CardSecurityCode 
                );
                paymentRequest.Signature = GetSignatureFromObject(paymentRequest, Options.ShaRequestPhrase);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(paymentRequest, Settings), Encoding.UTF8, "application/json");
                return jsonContent;
            }
            Result<PaymentStatuses> GetStatus(PayfortPaymentResponse model)
            {
                switch (model.ResponseCode)
                {
                    case PayfortConst.PaymentSuccessResponseCode: return Result.Ok(PaymentStatuses.Success);
                    case PayfortConst.PaymentSecure3dResponseCode: return Result.Ok(PaymentStatuses.Secure3d);
                    default: return Result.Fail<PaymentStatuses>($"Payment error. {model.ResponseCode}: {model.ResponseMessage}");
                }
            }
                
        }

        private static string ToPayfortBoolean(bool value) => 
            value ? "YES" : "NO";

        private static string ToPayfortAmount(decimal amount, Currencies currency) => 
            (amount * PaymentConstants.Multipliers[currency]).ToString("F0");

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

        private static string GetSignature(Dictionary<string, string> dict, string pass, string[] fieldsToIgnore = null)
        {
            var filteredValues = dict
                .Where(kv => kv.Key != "signature" && // Do not include signature in all cases
                    (fieldsToIgnore == null || !fieldsToIgnore.Contains(kv.Key)) &&
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

        private static string GetSignatureFromObject<T>(T model, string pass, string[] fieldsToIgnore = null)
        {
            var values = GetDictFromObject(model);
            return GetSignature(values, pass, fieldsToIgnore);
        }

        private static string GetSignatureFromQuery(NameValueCollection query, string pass, string[] fieldsToIgnore = null)
        {
            var dict = query.AllKeys.ToDictionary(k => k, k => query.GetValues(k)?.FirstOrDefault());
            return GetSignature(dict, pass, fieldsToIgnore);
        }

        private static string GetSignatureFromJson(string json, string pass, string[] fieldsToIgnore = null)
        {
            var jObject = JsonConvert.DeserializeObject<JObject>(json, Settings);
            var dict = jObject.Properties().ToDictionary(p => p.Name, p => p.Value.Value<object>()?.ToString());
            return GetSignature(dict, pass, fieldsToIgnore);
        }

        private readonly ILogger<PayfortService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IOptions<PayfortOptions> _options;
        private PayfortOptions Options => _options.Value;

        private static readonly string[] IgnoredForTokenizationFields = new[]
        {
            "card_security_code",
            "card_number",
            "expiry_date",
            "card_holder_name",
            "remember_me"
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
