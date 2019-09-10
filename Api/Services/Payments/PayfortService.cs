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

        public async Task<Result<TokenizationInfo>> Tokenization(TokenizationRequest request)
        {
            // TODO: Refactor after tests in test eviroment
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    var allKeys = new Dictionary<string, string>()
                    {
                        { "service_command", "TOKENIZATION" },
                        { "access_code ", Options.AccessCode },
                        { "merchant_identifier", Options.Identifier },
                        { "merchant_reference", Options.Reference },
                        { "language", request.Language },
                        { "expiry_date", request.ExpiryDate },
                        { "card_number", request.CardNumber },
                        { "card_security_code", request.CardSecurityCode },
                        { "card_holder_name", request.CardHolderName },
                        { "remember_me", ToValue(request.RememberMe) },
                        { "return_url ", Options.ReturnUrl },
                    };

                    allKeys["signature"] = GetSignature(allKeys.ToDictionary(kv => kv.Key, kv => (object)kv.Value), Options.SHARequestPhrase);

                    var requestContent = new FormUrlEncodedContent(allKeys);

                    requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    using (var response = await client.PostAsync(Options.TokenizationUrl, requestContent).ConfigureAwait(false))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<TokenizationInfo>(content);
                        var model = JsonConvert.DeserializeObject<PayfortTokenizationResponse>(content, _settings);

                        if (model == null)
                            return Result.Fail<TokenizationInfo>($"Invalid Tokenization response: {content}");

                        var signature = GetSignature(model, Options.SHAResponsePhrase);

                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Tokenization error: Invalid response signature. content: {0}", content);
                            return Result.Fail<TokenizationInfo>($"Tokenization error: Invalid response signature");
                        }

                        if (model.Status != "18000")
                            return Result.Fail<TokenizationInfo>($"Tokenization error. {model.Status}: {model.ResponseMessage}");

                        return Result.Ok(new TokenizationInfo(model.ExpiryDate, model.CardNnumber, model.TokenName, model.CardHolderName));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDataProviderClientException(ex);
                return Result.Fail<TokenizationInfo>(ex.Message);
            }
        }

        public async Task<Result<PaymentInfo>> Payment(PaymentRequest request)
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
                        Amount = GetAmount(request.Amount, request.Currency),
                        CardSecurityCode = request.CardSecurityCode,
                        Command = "PURCHASE",
                        Currency = request.Currency.ToString(),
                        CustomerName = request.CustomerName,
                        CustomerEmail = request.CustomerEmail,
                        CustomerIp = request.CustomerIp,
                        Language = request.Language,
                        RememberMe = ToValue(request.RememberMe),
                        ReturnUrl = Options.ReturnUrl,
                        SettlementReference = request.ReferenceCode,
                        TokenName = request.TokenName
                    };
                    paymentRequest.Signature = GetSignature(JObject.FromObject(paymentRequest), Options.SHARequestPhrase);
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(paymentRequest, _settings), Encoding.UTF8, "application/json");
                    using (var response = await client.PostAsync(Options.PaymentUrl, jsonContent).ConfigureAwait(false))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<PaymentInfo>(content);
                        var model = JsonConvert.DeserializeObject<PayfortPaymentResponse>(content, _settings);

                        if (model == null)
                            return Result.Fail<PaymentInfo>($"Invalid payfort payment response: {content}");

                        var signature = GetSignature(model, Options.SHAResponsePhrase);
                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Payment error: Invalid response signature. content: {0}", content);
                            return Result.Fail<PaymentInfo>($"Payment error: Invalid response signature");
                        }
                        // TODO: not success or 3dSecure
                        if (model.Status != "00000")
                            return Result.Fail<PaymentInfo>($"Payment error. {model.Status}: {model.ResponseMessage}");

                        return Result.Ok(new PaymentInfo(model.Secure3D, model.SettlementReference, model.AuthorizationCode, model.FortId, model.ExpiryDate, model.CardNumber, model.CardHolderName));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDataProviderClientException(ex);
                return Result.Fail<PaymentInfo>(ex.Message);
            }
        }

        private string ToValue(bool value) => value ? "YES" : "NO";
        private decimal GetAmount(decimal amount, Currencies currency) => amount * PaymentContants.Multipliers[currency];
        private string GetSignature(object model, string pass)
        {
            var jObject = JObject.FromObject(model);
            var dict = jObject.Properties().ToDictionary(p => p.Name, p => p.Value.Value<object>());
            return GetSignature(dict, pass);
        }
        private string GetSignature(Dictionary<string, object> values, string pass)
        {
            var filteredValues = values.Where(kv => !_ignoreForSignature.Contains(kv.Key) && kv.Value != null).OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}");
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

        readonly static string[] _ignoreForSignature = new[] { "card_security_code", "card_number", "expiry_date", "card_holder_name", "remember_me", "signature" };

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
    }
}
