using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardManagementService
    {
        Task<List<CreditCardInfo>> Get(CustomerInfo customerInfo);

        Task<Result> Delete(int cardId, CustomerInfo customerInfo);

        TokenizationSettings GetTokenizationSettings();

        Task<Result<string>> GetToken(int cardId, CustomerInfo customerInfo);

        Task Save(CreditCardInfo cardInfo, string token, CustomerInfo customerInfo);
    }
}