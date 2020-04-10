using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardMoneyAuthorizationService
    {
        Task<Result<CreditCardPaymentResult>> ProcessPaymentResponse(CreditCardPaymentResult paymentResponse,
            Currencies currency,
            CustomerInfo customer);


        Task<Result<CreditCardPaymentResult>> AuthorizeMoneyForService(CreditCardPaymentRequest request,
            CustomerInfo customer);
    }
}