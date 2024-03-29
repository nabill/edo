using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.Payments.NGenius;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.PaymentLinks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using CreditCardPaymentRequest = HappyTravel.Edo.Api.Models.Payments.Payfort.CreditCardPaymentRequest;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public class PaymentLinksProcessingService : IPaymentLinksProcessingService
    {
        public PaymentLinksProcessingService(IPayfortService payfortService,
            IPayfortResponseParser payfortResponseParser,
            IPaymentLinksStorage storage,
            IPayfortSignatureService signatureService,
            IOptions<PayfortOptions> payfortOptions,
            IPaymentLinkNotificationService notificationService,
            IEntityLocker locker,
            INGeniusClient nGeniusClient)
        {
            _payfortService = payfortService;
            _payfortResponseParser = payfortResponseParser;
            _storage = storage;
            _signatureService = signatureService;
            _notificationService = notificationService;
            _locker = locker;
            _payfortOptions = payfortOptions.Value;
            _nGeniusClient = nGeniusClient;
        }


        public Task<Result<PaymentResponse>> Pay(string code, string token, string ip, string languageCode)
        {
            return GetLink(code)
                .Bind(link => ProcessPay(link, code, token, ip, languageCode));
        }
        
        
        public async Task<Result<NGeniusPaymentResponse>> Pay(string code, NGeniusPayByLinkRequest request, string ip, string languageCode)
        {
            var (_, isFailure, link, error) = await _storage.Get(code);
            if (isFailure)
                return Result.Failure<NGeniusPaymentResponse>(error);

            return await _nGeniusClient.CreateOrder(orderType: OrderTypes.Sale,
                    referenceCode: link.ReferenceCode,
                    currency: link.Currency,
                    price: link.Amount,
                    email: request.EmailAddress,
                    billingAddress: request.BillingAddress)
                .Bind(SaveExternalId);


            async Task<Result<NGeniusPaymentResponse>> SaveExternalId(NGeniusPaymentResponse response)
            {
                await _storage.SetExternalId(code, response.OrderReference);
                return response;
            }
        }
        
        
        public async Task<Result<StatusResponse>> RefreshStatus(string code)
        {
            return await GetLink(code)
                .Bind(GetStatus)
                .Bind(StorePaymentResult);
            
            // TODO: add sending confirmation


            Task<Result<PaymentStatuses>> GetStatus(PaymentLink paymentLink)
            {
                return _nGeniusClient.GetStatus(paymentLink.ExternalId, paymentLink.Currency);
            }


            async Task<Result<StatusResponse>> StorePaymentResult(PaymentStatuses status)
            {
                var creditCardPaymentStatus = status == PaymentStatuses.Captured
                    ? CreditCardPaymentStatuses.Success
                    : CreditCardPaymentStatuses.Failed;
                
                await _storage.UpdatePaymentStatus(code, new PaymentResponse(string.Empty, 
                    creditCardPaymentStatus, 
                    string.Empty));
                
                return new StatusResponse(creditCardPaymentStatus);
            }
        }


        public Task<Result<PaymentResponse>> ProcessPayfortWebhook(string code, JObject response)
        {
            return Result.Success()
                .BindWithLock(_locker, typeof(PaymentLink), code, () => Result.Success()
                    .Bind(GetLink)
                    .Bind(ProcessResponse));

            Task<Result<PaymentLink>> GetLink() => this.GetLink(code);

            Task<Result<PaymentResponse>> ProcessResponse(PaymentLink link) => this.ProcessResponse(link.ToLinkData(), code, response);
        }


        public Task<Result<PaymentResponse>> ProcessNGeniusWebhook(string code, CreditCardPaymentStatuses status)
        {
            return Result.Success()
                .BindWithLock(_locker, typeof(PaymentLink), code, () => Result.Success()
                    .Bind(GetLink)
                    .Bind(ProcessResponse));

            Task<Result<PaymentLink>> GetLink() 
                => this.GetLink(code);

            Task<Result<PaymentResponse>> ProcessResponse(PaymentLink link) => this.ProcessResponse(link.ToLinkData(), code, status);
        }


        public Task<Result<string>> CalculateSignature(string code, string merchantReference, string fingerprint, string languageCode)
        {
            return GetLink(code)
                .Bind(GetSignature);


            Result<string> GetSignature(PaymentLink paymentLinkData)
            {
                var signingData = new Dictionary<string, string>
                {
                    {"service_command", "TOKENIZATION"},
                    {"access_code", _payfortOptions.AccessCode},
                    {"merchant_identifier", _payfortOptions.Identifier},
                    {"merchant_reference", merchantReference},
                    {"language", languageCode},
                    {"device_fingerprint", fingerprint},
                    {"return_url", $"{_payfortOptions.ResultUrl}/{paymentLinkData.ReferenceCode}"},
                    {"signature", string.Empty}
                };
                return _signatureService.Calculate(signingData, SignatureTypes.Request);
            }
        }


        private Task<Result<PaymentResponse>> ProcessPay(PaymentLink link, string code, string token, string ip, string languageCode)
        {
            return Pay()
                .CheckIf(IsPaymentComplete, CheckPaymentAmount)
                .TapIf(IsPaymentComplete, SendConfirmation)
                .Map(ToPaymentResponse)
                .Tap(StorePaymentResult);


            Task<Result<CreditCardPaymentResult>> Pay()
                => _payfortService.Pay(new CreditCardPaymentRequest(
                    link.Amount,
                    link.Currency,
                    new PaymentTokenInfo(token, PaymentTokenTypes.OneTime),
                    null,
                    link.Email,
                    ip,
                    link.ReferenceCode,
                    languageCode,
                    true,
                    // Is not needed for new card
                    null,
                    link.ReferenceCode));


            Result CheckPaymentAmount(CreditCardPaymentResult paymentResult)
            {
                return link.Amount == paymentResult.Amount
                    ? Result.Success()
                    : Result.Failure($"Payment amount invalid, expected '{link.Amount}' but was '{paymentResult.Amount}'");
            }

            bool IsPaymentComplete(CreditCardPaymentResult paymentResult) => paymentResult.Status == CreditCardPaymentStatuses.Success;

            Task SendConfirmation() => this.SendConfirmation(link.ToLinkData());

            PaymentResponse ToPaymentResponse(CreditCardPaymentResult cr) => new PaymentResponse(cr.Secure3d, cr.Status, cr.Message);

            Task StorePaymentResult(PaymentResponse response) => _storage.UpdatePaymentStatus(code, response);
        }


        private Task<Result<PaymentResponse>> ProcessResponse(PaymentLinkData link, string code, JObject response)
        {
            return ParseResponse()
                .TapIf(ShouldSendReceipt, parsedResponse => SendReceipt())
                .Map(StorePaymentResult);


            Result<PaymentResponse> ParseResponse()
            {
                var (_, isFailure, cardPaymentResult, error) = _payfortResponseParser.ParsePaymentResponse(response);
                if (isFailure)
                    return Result.Failure<PaymentResponse>(error);

                return Result.Success(new PaymentResponse(cardPaymentResult.Secure3d,
                    cardPaymentResult.Status,
                    cardPaymentResult.Message));
            }


            bool ShouldSendReceipt(PaymentResponse parsedResponse)
            {
                return parsedResponse.Status == CreditCardPaymentStatuses.Success &&
                    IsNotAlreadyPaid(link);

                static bool IsNotAlreadyPaid(PaymentLinkData link) => link.CreditCardPaymentStatus != CreditCardPaymentStatuses.Success;
            }


            Task SendReceipt() => this.SendConfirmation(link);


            async Task<PaymentResponse> StorePaymentResult(PaymentResponse paymentResponse)
            {
                await _storage.UpdatePaymentStatus(code, paymentResponse);
                return paymentResponse;
            }
        }
        
        
        private Task<Result<PaymentResponse>> ProcessResponse(PaymentLinkData link, string code, CreditCardPaymentStatuses status)
        {
            return GenerateResponse()
                .TapIf(ShouldSendReceipt, parsedResponse => SendReceipt())
                .Map(StorePaymentResult);
            
            
            Result<PaymentResponse> GenerateResponse()
            {
                return Result.Success(new PaymentResponse(string.Empty,
                    status,
                    string.Empty));
            }


            bool ShouldSendReceipt(PaymentResponse parsedResponse)
            {
                return status == CreditCardPaymentStatuses.Success &&
                    IsNotAlreadyPaid(link);

                static bool IsNotAlreadyPaid(PaymentLinkData link) 
                    => link.CreditCardPaymentStatus != CreditCardPaymentStatuses.Success;
            }


            Task SendReceipt() 
                => SendConfirmation(link);


            async Task<PaymentResponse> StorePaymentResult(PaymentResponse paymentResponse)
            {
                await _storage.UpdatePaymentStatus(code, paymentResponse);
                return paymentResponse;
            }
        }


        private Task<Result> SendConfirmation(PaymentLinkData link) => _notificationService.SendPaymentConfirmation(link);


        private Task<Result<PaymentLink>> GetLink(string code) => _storage.Get(code);

        private readonly IPaymentLinksStorage _storage;
        private readonly IEntityLocker _locker;
        private readonly PayfortOptions _payfortOptions;
        private readonly IPayfortService _payfortService;
        private readonly IPayfortResponseParser _payfortResponseParser;
        private readonly IPayfortSignatureService _signatureService;
        private readonly IPaymentLinkNotificationService _notificationService;
        private readonly INGeniusClient _nGeniusClient;
    }
}