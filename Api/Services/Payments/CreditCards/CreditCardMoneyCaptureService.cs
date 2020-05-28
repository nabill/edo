using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.AuditEvents;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardMoneyCaptureService : ICreditCardMoneyCaptureService
    {
        public CreditCardMoneyCaptureService(IPayfortService payfortService,
            ICreditCardAuditService creditCardAuditService)
        {
            _payfortService = payfortService;
            _creditCardAuditService = creditCardAuditService;
        }


        public async Task<Result<CreditCardCaptureResult>> Capture(CreditCardCaptureMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            string maskedNumber,
            Currencies currency,
            AgentInfo customer)
        {
            return await CaptureInPayfort()
                .Tap(WriteAuditLog);

            Task<Result<CreditCardCaptureResult>> CaptureInPayfort() => _payfortService.Capture(request);

            Task WriteAuditLog(CreditCardCaptureResult captureResult)
            {
                var eventData = new CreditCardLogEventData($"Capture money for the payment '{request.MerchantReference}'",
                    captureResult.ExternalCode,
                    captureResult.Message,
                    paymentInfo.InternalReferenceCode);

                return _creditCardAuditService.Write(CreditCardEventType.Capture,
                    maskedNumber,
                    request.Amount,
                    new UserInfo(customer.AgentId, UserTypes.Agent),
                    eventData,
                    request.MerchantReference,
                    customer.AgentId,
                    currency);
            }
        }


        public async Task<Result<CreditCardVoidResult>> Void(CreditCardVoidMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            string maskedNumber,
            MoneyAmount moneyAmount,
            string referenceCode,
            AgentInfo customer)
        {
            return await VoidInPayfort()
                .Tap(WriteAuditLog);

            Task<Result<CreditCardVoidResult>> VoidInPayfort() => _payfortService.Void(request);

            Task WriteAuditLog(CreditCardVoidResult voidResult)
            {
                var eventData = new CreditCardLogEventData($"Void money for the payment '{referenceCode}'", 
                    voidResult.ExternalCode,
                    voidResult.Message,
                    paymentInfo.InternalReferenceCode);
                
                return _creditCardAuditService.Write(CreditCardEventType.Void,
                    maskedNumber,
                    moneyAmount.Amount,
                    new UserInfo(customer.AgentId, UserTypes.Agent),
                    eventData,
                    referenceCode,
                    customer.AgentId,
                    moneyAmount.Currency);
            }
        }

        private readonly ICreditCardAuditService _creditCardAuditService;
        private readonly IPayfortService _payfortService;
    }
}