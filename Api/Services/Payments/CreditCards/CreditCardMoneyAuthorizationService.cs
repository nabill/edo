using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.AuditEvents;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardMoneyAuthorizationService : ICreditCardMoneyAuthorizationService
    {
        public CreditCardMoneyAuthorizationService(IPayfortService payfortService,
            IDateTimeProvider dateTimeProvider,
            IPaymentNotificationService notificationService,
            ICreditCardAuditService creditCardAuditService)
        {
            _payfortService = payfortService;
            _dateTimeProvider = dateTimeProvider;
            _notificationService = notificationService;
            _creditCardAuditService = creditCardAuditService;
        }


        public Task<Result<CreditCardPaymentResult>> ProcessPaymentResponse(CreditCardPaymentResult paymentResponse,
            Currencies currency,
            AgentInfo customer)
        {
            return CheckPaymentStatusNotFailed(paymentResponse)
                .TapIf(IsPaymentComplete, cardPaymentResult => WriteAuditLog())
                .TapIf(IsPaymentComplete, cardPaymentResult => SendBillToCustomer());


            Result<CreditCardPaymentResult> CheckPaymentStatusNotFailed(CreditCardPaymentResult payment)
                => payment.Status == CreditCardPaymentStatuses.Failed
                    ? Result.Failure<CreditCardPaymentResult>($"Payment error: {payment.Message}")
                    : Result.Ok(payment);


            bool IsPaymentComplete(CreditCardPaymentResult cardPaymentResult) => cardPaymentResult.Status == CreditCardPaymentStatuses.Success;

            Task WriteAuditLog() => WriteAuthorizeAuditLog(paymentResponse, customer, currency);


            Task SendBillToCustomer()
            {
                return this.SendBillToCustomer(customer,
                    new MoneyAmount(paymentResponse.Amount, currency),
                    paymentResponse.ReferenceCode);
            }
        }


        public Task<Result<CreditCardPaymentResult>> AuthorizeMoneyForService(CreditCardPaymentRequest request,
            AgentInfo agent)
        {
            return AuthorizeInPaymentSystem(request)
                .Tap(WriteAuditLog)
                .TapIf(IsPaymentComplete, SendBillToCustomer);


            async Task<Result<CreditCardPaymentResult>> AuthorizeInPaymentSystem(CreditCardPaymentRequest paymentRequest)
            {
                var (_, isFailure, paymentResult, error) = await _payfortService.Authorize(paymentRequest);
                if (isFailure)
                    return Result.Failure<CreditCardPaymentResult>(error);

                return paymentResult.Status == CreditCardPaymentStatuses.Failed
                    ? Result.Failure<CreditCardPaymentResult>($"Payment error: {paymentResult.Message}")
                    : Result.Ok(paymentResult);
            }


            bool IsPaymentComplete(CreditCardPaymentResult paymentResult) => paymentResult.Status == CreditCardPaymentStatuses.Success;


            Task SendBillToCustomer(CreditCardPaymentResult paymentResult)
            {
                return this.SendBillToCustomer(agent,
                    new MoneyAmount(request.Amount, request.Currency),
                    request.ReferenceCode);
            }


            Task WriteAuditLog(CreditCardPaymentResult result) => WriteAuthorizeAuditLog(result, agent, request.Currency);
        }


        private Task SendBillToCustomer(AgentInfo customer, MoneyAmount amount, string referenceCode)
        {
            return _notificationService.SendBillToCustomer(new PaymentBill(customer.Email,
                amount.Amount,
                amount.Currency,
                _dateTimeProvider.UtcNow(),
                PaymentMethods.CreditCard,
                referenceCode,
                $"{customer.LastName} {customer.FirstName}"));
        }


        private Task WriteAuthorizeAuditLog(CreditCardPaymentResult payment, AgentInfo agent, Currencies currency)
        {
            var eventData = new CreditCardLogEventData($"Authorize money for the payment '{payment.ReferenceCode}'",
                payment.ExternalCode,
                payment.Message,
                payment.MerchantReference);

            return _creditCardAuditService.Write(CreditCardEventType.Authorize,
                payment.CardNumber,
                payment.Amount,
                new UserInfo(agent.AgentId, UserTypes.Agent),
                eventData,
                payment.ReferenceCode,
                agent.AgentId,
                currency);
        }


        private readonly IPayfortService _payfortService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPaymentNotificationService _notificationService;
        private readonly ICreditCardAuditService _creditCardAuditService;
    }
}