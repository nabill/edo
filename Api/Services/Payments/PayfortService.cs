using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

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
                var requestContent = GetSignedContent();
                using (var client = _clientFactory.CreateClient(HttpClientNames.Payfort))
                {
                    using (var response = await client.PostAsync(_options.PaymentUrl, requestContent))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                            return Result.Fail<CreditCardPaymentResult>(content);
                        var responseObject = JObject.Parse(content);
                        return ProcessPaymentResponse(responseObject);
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
        }

        public Result<CreditCardPaymentResult> ProcessPaymentResponse(JObject response)
        {
            var model = response.ToObject<PayfortPaymentResponse>(Serializer);

            if (model == null)
                return Result.Fail<CreditCardPaymentResult>($"Invalid payfort payment response: {response}");

            var signature = _signatureService.Calculate(response, _options.ShaResponsePhrase);
            if (signature != model.Signature)
            {
                _logger.LogError("Payfort Payment error: Invalid response signature. content: {0}", response);
                return Result.Fail<CreditCardPaymentResult>($"Payment error: Invalid response signature");
            }

            var (_, isFailed, status, error) = CheckStatus(model);
            if (isFailed)
                return Result.Fail<CreditCardPaymentResult>(error);

            return Result.Ok(new CreditCardPaymentResult(model.Secure3d, model.SettlementReference, model.AuthorizationCode, model.FortId,
                model.ExpirationDate, model.CardNumber, status));
            
            Result<PaymentStatuses> CheckStatus(PayfortPaymentResponse payment)
            {
                switch (payment.ResponseCode)
                {
                    case PayfortConstants.PaymentSuccessResponseCode: return Result.Ok(PaymentStatuses.Success);
                    case PayfortConstants.PaymentSecure3dResponseCode: return Result.Ok(PaymentStatuses.Secure3d);
                    default: return Result.Fail<PaymentStatuses>($"Payment error. {payment.ResponseCode}: {payment.ResponseMessage}");
                }
            }
        }

        private static string ToPayfortBoolean(bool value) => 
            value ? "YES" : "NO";

        private static string ToPayfortAmount(decimal amount, Currencies currency) => 
            (amount * PayfortConstants.Multipliers[currency]).ToString("F0");
        
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
        private static readonly  JsonSerializer Serializer = JsonSerializer.Create(Settings);
    }
}
