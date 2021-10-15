using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.AuditEvents;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.NGenius;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardMoneyCaptureService : ICreditCardMoneyCaptureService
    {
        public CreditCardMoneyCaptureService(IPayfortService payfortService,
            INGeniusPaymentService nGeniusPaymentService,
            ICreditCardAuditService creditCardAuditService)
        {
            _payfortService = payfortService;
            _nGeniusPaymentService = nGeniusPaymentService;
            _creditCardAuditService = creditCardAuditService;
        }


        public async Task<Result<CreditCardCaptureResult>> Capture(CreditCardCaptureMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            PaymentProcessors paymentProcessor,
            string maskedNumber,
            Currencies currency,
            ApiCaller apiCaller,
            int agentId)
        {
            return await Capture()
                .Tap(WriteAuditLog);

            Task<Result<CreditCardCaptureResult>> Capture()
            {
                return paymentProcessor switch
                {
                    PaymentProcessors.Payfort => _payfortService.Capture(request),
                    PaymentProcessors.NGenius => _nGeniusPaymentService.Capture(paymentInfo.ExternalId, paymentInfo.InternalReferenceCode, request.Amount.ToMoneyAmount(request.Currency)),
                    _ => throw new NotSupportedException($"Payment processor `{nameof(paymentProcessor)}` not supported")
                };
            }


            Task WriteAuditLog(CreditCardCaptureResult captureResult)
            {
                var eventData = new CreditCardLogEventData($"Capture money for the payment '{request.MerchantReference}'",
                    captureResult.ExternalCode,
                    captureResult.Message,
                    paymentInfo.InternalReferenceCode);

                return _creditCardAuditService.Write(CreditCardEventType.Capture,
                    maskedNumber,
                    request.Amount,
                    apiCaller,
                    eventData,
                    request.MerchantReference,
                    agentId,
                    currency);
            }
        }


        public async Task<Result<CreditCardVoidResult>> Void(CreditCardVoidMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            PaymentProcessors paymentProcessor,
            string maskedNumber,
            MoneyAmount moneyAmount,
            string referenceCode,
            ApiCaller apiCaller,
            int agentId)
        {
            return await Void()
                .Tap(WriteAuditLog);

            Task<Result<CreditCardVoidResult>> Void()
            {
                return paymentProcessor switch
                {
                    PaymentProcessors.Payfort => _payfortService.Void(request) ,
                    PaymentProcessors.NGenius => _nGeniusPaymentService.Void(paymentInfo.ExternalId, paymentInfo.InternalReferenceCode, moneyAmount.Currency),
                    _ => throw new NotSupportedException($"Payment processor {nameof(paymentProcessor)} not supported")
                };
            }


            Task WriteAuditLog(CreditCardVoidResult voidResult)
            {
                var eventData = new CreditCardLogEventData($"Void money for the payment '{referenceCode}'",
                    voidResult.ExternalCode,
                    voidResult.Message,
                    paymentInfo.InternalReferenceCode);

                return _creditCardAuditService.Write(CreditCardEventType.Void,
                    maskedNumber,
                    moneyAmount.Amount,
                    apiCaller,
                    eventData,
                    referenceCode,
                    agentId,
                    moneyAmount.Currency);
            }
        }


        private readonly ICreditCardAuditService _creditCardAuditService;
        private readonly IPayfortService _payfortService;
        private readonly INGeniusPaymentService _nGeniusPaymentService;
    }
}