using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.AuditEvents;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.NGenius;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Extensions;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardMoneyRefundService : ICreditCardMoneyRefundService
    {
        public CreditCardMoneyRefundService(IPayfortService payfortService,
            ICreditCardAuditService creditCardAuditService,
            INGeniusRefundService nGeniusRefundService)
        {
            _payfortService = payfortService;
            _creditCardAuditService = creditCardAuditService;
            _nGeniusRefundService = nGeniusRefundService;
        }

        public async Task<Result<CreditCardRefundResult>> Refund(CreditCardRefundMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            PaymentProcessors paymentProcessor,
            string maskedNumber,
            string referenceCode,
            int paymentId,
            ApiCaller apiCaller,
            int agentId)
        {
            return await RefundInPayfort()
                .Tap(WriteAuditLog);

            async Task<Result<CreditCardRefundResult>> RefundInPayfort()
            {
                return request.Amount.IsGreaterThan(0m)
                    ? paymentProcessor == PaymentProcessors.Payfort 
                        ? await _payfortService.Refund(request)
                        : await _nGeniusRefundService.Refund(paymentId, request.Amount.ToMoneyAmount(request.Currency), paymentInfo.ExternalId, request.MerchantReference)
                    : new CreditCardRefundResult(default, default, request.MerchantReference);
            }


            Task WriteAuditLog(CreditCardRefundResult refundResult)
            {
                var eventData = new CreditCardLogEventData($"Refund money for the payment '{referenceCode}'",
                    refundResult.ExternalCode,
                    refundResult.Message,
                    paymentInfo.InternalReferenceCode);

                return _creditCardAuditService.Write(CreditCardEventType.Refund,
                    maskedNumber,
                    request.Amount,
                    apiCaller,
                    eventData,
                    referenceCode,
                    agentId,
                    request.Currency);
            }
        }

        private readonly ICreditCardAuditService _creditCardAuditService;
        private readonly IPayfortService _payfortService;
        private readonly INGeniusRefundService _nGeniusRefundService;
    }
}