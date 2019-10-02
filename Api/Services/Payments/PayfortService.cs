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
using System.Text;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PayfortService : IPayfortService
    {
        public PayfortService(ILogger<PayfortService> logger, IHttpClientFactory clientFactory, IOptions<PayfortOptions> options,
            IPayfortSignatureService signatureService)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _options = options.Value;
            _signatureService = signatureService;
        }

        public async Task<Result<CreditCardPaymentResult>> Pay(CreditCardPaymentRequest request)
        {
            try
            {
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    var requestContent = GetSignedContent();
        
                    using (var response = await client.PostAsync(_options.PaymentUrl, requestContent))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<CreditCardPaymentResult>(content);
                        var model = JsonConvert.DeserializeObject<PayfortPaymentResponse>(content, Settings);

                        if (model == null)
                            return Result.Fail<CreditCardPaymentResult>($"Invalid payfort payment response: {content}");

                        var responseObject = JObject.Parse(content);
                        var signature = _signatureService.Calculate(responseObject, _options.ShaResponsePhrase);
                        if (signature != model.Signature)
                        {
                            _logger.LogError("Payfort Payment error: Invalid response signature. content: {0}", content);
                            return Result.Fail<CreditCardPaymentResult>($"Payment error: Invalid response signature");
                        }

                        var (_, isFailed, status, error) = GetStatus(model);
                        if (isFailed)
                            return Result.Fail<CreditCardPaymentResult>(error);

                        return Result.Ok(new CreditCardPaymentResult(model.Secure3d, model.SettlementReference, model.AuthorizationCode, model.FortId,
                            model.ExpirationDate, model.CardNumber, status));
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
                    accessCode: _options.AccessCode,
                    merchantIdentifier: _options.Identifier,
                    merchantReference: request.ReferenceCode,
                    amount: ToPayfortAmount(request.Amount, request.Currency),
                    currency: request.Currency.ToString(),
                    customerName: request.CustomerName,
                    customerEmail: request.CustomerEmail,
                    customerIp: request.CustomerIp,
                    language: request.LanguageCode,
                    rememberMe: ToPayfortBoolean(!request.IsOneTime),
                    returnUrl: _options.ReturnUrl,
                    settlementReference: request.ReferenceCode,
                    tokenName: request.Token,
                    // There are error "Invalid extra parameters" if secureCode filled for One time token
                    cardSecurityCode: request.IsOneTime ? null : request.CardSecurityCode
                );
                var jObject = JObject.FromObject(paymentRequest, JsonSerializer.Create(Settings));;
                paymentRequest.Signature = _signatureService.Calculate(jObject, _options.ShaRequestPhrase);
                var json = JsonConvert.SerializeObject(paymentRequest, Settings);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
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
        
        private readonly ILogger<PayfortService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly PayfortOptions _options;
        private readonly IPayfortSignatureService _signatureService;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
    }
}
