using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments.CreditCard;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface ICreditCardService
    {
        Task<List<CreditCardInfo>> Get(Customer customer, Company company);

        Task<Result> IsAvailable(int cardId, Customer customer, Company company);

        Task<Result<CreditCardInfo>> Create(CreateCreditCardRequest request, int ownerId, string languageCode);

        Task<Result> Delete(int cardId, Customer customer, Company company);
    }
}
