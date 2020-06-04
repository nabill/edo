using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.PaymentLinks;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public class PaymentLinksProcessingService : IPaymentLinksProcessingService
    {
        public PaymentLinksProcessingService(IPayfortService payfortService,
            IPayfortResponseParser payfortResponseParser,
            IPaymentLinkService linkService,
            IPayfortSignatureService signatureService,
            IOptions<PayfortOptions> payfortOptions,
            IPaymentNotificationService notificationService,
            IDateTimeProvider dateTimeProvider,
            IEntityLocker locker)
        {
            _payfortService = payfortService;
            _payfortResponseParser = payfortResponseParser;
            _linkService = linkService;
            _signatureService = signatureService;
            _notificationService = notificationService;
            _dateTimeProvider = dateTimeProvider;
            _locker = locker;
            _payfortOptions = payfortOptions.Value;
        }


        public Task<Result<PaymentResponse>> Pay(string code, string token, string ip, string languageCode)
        {
            return GetLink(code)
                .Bind(link => ProcessPay(link, code, token, ip, languageCode));
        }


        public Task<Result<PaymentResponse>> ProcessResponse(string code, JObject response)
        {
            return LockLink()
                .Bind(GetLink)
                .Bind(ProcessResponse)
                .Finally(UnlockLink);

            Task<Result> LockLink() => _locker.Acquire<PaymentLink>(code, nameof(PaymentLinksProcessingService));

            Task<Result<PaymentLinkData>> GetLink() => this.GetLink(code);

            Task<Result<PaymentResponse>> ProcessResponse(PaymentLinkData link) => this.ProcessResponse(link, code, response);


            async Task<Result<PaymentResponse>> UnlockLink(Result<PaymentResponse> paymentResponse)
            {
                await _locker.Release<PaymentLink>(code);
                return paymentResponse;
            }
        }


        public Task<Result<string>> CalculateSignature(string code, string merchantReference, string fingerprint, string languageCode)
        {
            return GetLink(code)
                .Bind(GetSignature);


            Result<string> GetSignature(PaymentLinkData paymentLinkData)
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


        private Task<Result<PaymentResponse>> ProcessPay(PaymentLinkData link, string code, string token, string ip, string languageCode)
        {
            return Pay()
                .TapIf(IsPaymentComplete, CheckPaymentAmount)
                .TapIf(IsPaymentComplete, SendReceiptToAgent)
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
                    ? Result.Ok()
                    : Result.Failure($"Payment amount invalid, expected '{link.Amount}' but was '{paymentResult.Amount}'");
            }

            bool IsPaymentComplete(CreditCardPaymentResult paymentResult) => paymentResult.Status == CreditCardPaymentStatuses.Success;

            Task SendReceiptToAgent() => this.SendReceiptToAgent(link);

            PaymentResponse ToPaymentResponse(CreditCardPaymentResult cr) => new PaymentResponse(cr.Secure3d, cr.Status, cr.Message);

            Task StorePaymentResult(PaymentResponse response) => _linkService.UpdatePaymentStatus(code, response);
        }


        private Task<Result<PaymentResponse>> ProcessResponse(PaymentLinkData link, string code, JObject response)
        {
            return ParseResponse()
                .TapIf(ShouldSendReceipt, parsedResponse => SendReceiptToAgent())
                .Map(StorePaymentResult);


            Result<PaymentResponse> ParseResponse()
            {
                var (_, isFailure, cardPaymentResult, error) = _payfortResponseParser.ParsePaymentResponse(response);
                if (isFailure)
                    return Result.Failure<PaymentResponse>(error);

                return Result.Ok(new PaymentResponse(cardPaymentResult.Secure3d,
                    cardPaymentResult.Status,
                    cardPaymentResult.Message));
            }


            bool ShouldSendReceipt(PaymentResponse parsedResponse)
            {
                return parsedResponse.Status == CreditCardPaymentStatuses.Success &&
                    IsNotAlreadyPaid(link);

                static bool IsNotAlreadyPaid(PaymentLinkData link) => link.CreditCardPaymentStatus != CreditCardPaymentStatuses.Success;
            }


            Task SendReceiptToAgent() => this.SendReceiptToAgent(link);


            async Task<PaymentResponse> StorePaymentResult(PaymentResponse paymentResponse)
            {
                await _linkService.UpdatePaymentStatus(code, paymentResponse);
                return paymentResponse;
            }
        }


        private Task SendReceiptToAgent(PaymentLinkData link)
            => _notificationService.SendReceiptToCustomer(new PaymentReceipt(
                link.Email,
                link.Amount,
                link.Currency,
                _dateTimeProvider.UtcNow(),
                PaymentMethods.CreditCard,
                link.ReferenceCode));


        private Task<Result<PaymentLinkData>> GetLink(string code) => _linkService.Get(code);

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPaymentLinkService _linkService;
        private readonly IEntityLocker _locker;
        private readonly IPaymentNotificationService _notificationService;
        private readonly PayfortOptions _payfortOptions;

        private readonly IPayfortService _payfortService;
        private readonly IPayfortResponseParser _payfortResponseParser;
        private readonly IPayfortSignatureService _signatureService;
    }
}