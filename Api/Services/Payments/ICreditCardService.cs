using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface ICreditCardService
    {
        Task<Result<CreditCardInfo[]>> GetAvailableCards();

        Task<Result> CanUseCard(int cardId);

        Task<Result> Create(CreditCard card);
    }
}
