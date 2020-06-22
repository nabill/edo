using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardMoneyAuthorizationService
    {
        Task<Result<CreditCardPaymentResult>> ProcessPaymentResponse(CreditCardPaymentResult paymentResponse,
            Currencies currency,
            AgentContext customer);


        Task<Result<CreditCardPaymentResult>> AuthorizeMoneyForService(CreditCardPaymentRequest request,
            AgentContext customer);
    }
}