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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
            // TODO: Refactor after tests in test environment
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    var requestContent = GetSignedContent();

                    using (var response = await client.PostAsync(Options.TokenizationUrl, requestContent))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<TokenizationInfo>(content);
                        var model = JsonConvert.DeserializeObject<PayfortTokenizationResponse>(content, Settings);

                        if (model == null)
                            return Result.Fail<TokenizationInfo>($"Invalid Tokenization response: {content}");

                        var signature = GetSignatureFromObject(model, Options.ShaResponsePhrase);

                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Tokenization error: Invalid response signature. content: {0}", content);
                            return Result.Fail<TokenizationInfo>($"Tokenization error: Invalid response signature");
                        }

                        if (IsFailed(model))
                            return Result.Fail<TokenizationInfo>($"Tokenization error. {model.Status}: {model.ResponseMessage}");

                        return Result.Ok(new TokenizationInfo(model.ExpirationDate, model.CardNnumber, model.TokenName, model.CardHolderName));
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
                var allKeys = new Dictionary<string, string>()
                    {
                        { "service_command", "TOKENIZATION" },
                        { "access_code ", Options.AccessCode },
                        { "merchant_identifier", Options.Identifier },
                        { "merchant_reference", Options.Reference },
                        { "language", request.Language },
                        { "expiry_date", request.ExpirationDate },
                        { "card_number", request.CardNumber },
                        { "card_security_code", request.CardSecurityCode },
                        { "card_holder_name", request.CardHolderName },
                        { "remember_me", ToPayfortBoolean(request.RememberMe) },
                        { "return_url ", Options.ReturnUrl },
                    };

                allKeys["signature"] = GetSignature(allKeys, Options.ShaRequestPhrase);

                var requestContent = new FormUrlEncodedContent(allKeys);

                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                return requestContent;
            }
            // TODO: Should be refactored after getting test environment
            bool IsFailed(PayfortTokenizationResponse model) => model.Status != "18000";
        }

        public async Task<Result<PaymentResult>> Pay(PaymentRequest request)
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
                        MerchantReference = Options.Reference,
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
                    paymentRequest.Signature = GetSignatureFromObject(JObject.FromObject(paymentRequest), Options.ShaRequestPhrase);
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(paymentRequest, Settings), Encoding.UTF8, "application/json");
                    using (var response = await client.PostAsync(Options.PaymentUrl, jsonContent))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<PaymentResult>(content);
                        var model = JsonConvert.DeserializeObject<PayfortPaymentResponse>(content, Settings);

                        if (model == null)
                            return Result.Fail<PaymentResult>($"Invalid payfort payment response: {content}");

                        var signature = GetSignatureFromObject(model, Options.ShaResponsePhrase);
                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Payment error: Invalid response signature. content: {0}", content);
                            return Result.Fail<PaymentResult>($"Payment error: Invalid response signature");
                        }
                        
                        if (IsFailed(model))
                            return Result.Fail<PaymentResult>($"Payment error. {model.Status}: {model.ResponseMessage}");

                        return Result.Ok(new PaymentResult(model.Secure3D, model.SettlementReference, model.AuthorizationCode, model.FortId,
                            model.ExpirationDate, model.CardNumber, model.CardHolderName));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogPayfortClientException(ex);
                return Result.Fail<PaymentResult>(ex.Message);
            }
            
            // TODO: Should be refactored after getting test environment. not success or 3dSecure
            bool IsFailed(PayfortPaymentResponse model) => model.Status != "00000";
        }

        private static string ToPayfortBoolean(bool value) => 
            value ? "YES" : "NO";

        private static decimal ToPayfortAmount(decimal amount, Currencies currency) => 
            amount * PaymentConstants.Multipliers[currency];

        private static string GetSignatureFromObject(object model, string pass)
        {
            var jObject = JObject.FromObject(model);
            var dict = jObject.Properties().ToDictionary(p => p.Name, p => p.Value.Value<object>()?.ToString());
            return GetSignature(dict, pass);
        }

        private static string GetSignature(Dictionary<string, string> values, string pass)
        {
            var filteredValues = values
                .Where(kv => !IgnoredForSignature.Contains(kv.Key) && kv.Value != null)
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}");
            var str = $"{pass}{string.Join("", filteredValues)}{pass}";
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash);
            }
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
            "remember_me", "signature"
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
