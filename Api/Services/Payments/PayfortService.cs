using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PayfortService : IPayfortService
    {
        private readonly ILogger<PayfortService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IOptions<PayfortOptions> _options;
        PayfortOptions Options => _options.Value;

        static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public PayfortService(ILogger<PayfortService> logger, IHttpClientFactory clientFactory, IOptions<PayfortOptions> options)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _options = options;
        }
        public async Task<Result<TokenizationInfo>> Tokenization(TokenizationRequest request, string lang)
        {
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    var signatureKeys = new Dictionary<string, string>
                    {
                        { "service_command", "TOKENIZATION" },
                        { "access_code ", Options.AccessCode },
                        { "merchant_identifier", Options.Identifier },
                        { "merchant_reference", Options.Reference },
                        { "language", lang },
                    };

                    var allKeys = new Dictionary<string, string>(signatureKeys)
                    {
                        { "expiry_date", request.ExpiryDate },
                        { "card_number", request.CardNumber },
                        { "card_security_code", request.CardSecurityCode },
                        { "card_holder_name", request.CardHolderName },
                        { "remember_me", ToValue(request.RememberMe) },
                        { "signature ", GetSignature(signatureKeys, Options.SHARequestPhrase) },
                        { "return_url ", Options.ReturnUrl },
                    };

                    var requestContent = new FormUrlEncodedContent(allKeys);

                    requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    using (var response = await client.PostAsync(Options.TokenizationUrl, requestContent).ConfigureAwait(false))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<TokenizationInfo>(content);

                        var model = JsonConvert.DeserializeObject<PayfortTokenizationResponse>(content, _settings);

                        if (model == null)
                            return Result.Fail<TokenizationInfo>($"Invalid payfort Tokenization response: {content}");
                        if (model.Status != "00000")
                            return Result.Fail<TokenizationInfo>($"Payfort Tokenization error: {model.ResponseMessage}");
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

        string ToValue(bool value) => value ? "YES" : "NO";

        string GetSignature(Dictionary<string, string> values, string pass)
        {
            var str = $"{pass}{string.Join("", values.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"))}{pass}";
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash);
            }
        }
    }
}
