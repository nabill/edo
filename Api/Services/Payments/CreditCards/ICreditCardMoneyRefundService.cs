using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardMoneyRefundService
    {
        Task<Result<CreditCardRefundResult>> Refund(CreditCardRefundMoneyRequest request,
            CreditCardPaymentInfo paymentInfo,
            PaymentProcessors paymentProcessor,
            string maskedNumber,
            string referenceCode,
            int paymentId,
            ApiCaller apiCaller,
            int agentId);
    }
}