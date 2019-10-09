using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Customers;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface ICreditCardService
    {
        Task<List<CreditCardInfo>> Get(CustomerInfo customerInfo);

        Task<Result<CreditCardInfo>> Save(SaveCreditCardRequest request, CustomerInfo customerInfo);

        Task<Result> Delete(int cardId, CustomerInfo customerInfo);

        TokenizationSettings GetTokenizationSettings();
    }
}
