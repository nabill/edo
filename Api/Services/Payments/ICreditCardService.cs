using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface ICreditCardService
    {
        Task<Result<List<CreditCardInfo>>> Get();

        Task<Result> IsAvailable(int cardId);

        Task<Result> Create(CreditCard card);
    }
}
