using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.AuditEvents;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;

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


        public async Task<Result<CreditCardCaptureResult>> CaptureMoney(CreditCardCaptureMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            string maskedNumber,
            Currencies currency,
            CustomerInfo customer)
        {
            return await CaptureInPayfort()
                .OnSuccess(WriteAuditLog);

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
                    new UserInfo(customer.CustomerId, UserTypes.Customer),
                    eventData,
                    request.MerchantReference,
                    customer.CustomerId,
                    currency);
            }
        }


        public async Task<Result<CreditCardVoidResult>> VoidMoney(CreditCardVoidMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            string maskedNumber,
            MoneyAmount moneyAmount,
            string referenceCode,
            CustomerInfo customer)
        {
            return await VoidInPayfort()
                .OnSuccess(WriteAuditLog);

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
                    new UserInfo(customer.CustomerId, UserTypes.Customer),
                    eventData,
                    referenceCode,
                    customer.CustomerId,
                    moneyAmount.Currency);
            }
        }

        private readonly ICreditCardAuditService _creditCardAuditService;
        private readonly IPayfortService _payfortService;
    }
}