using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Users;
using HappyTravel.Edo.Api.Models.Payments;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentProcessingService
    {
        Task<Result> AddMoney(int accountId, PaymentData paymentData, UserInfo user);

        Task<Result> ChargeMoney(int accountId, PaymentData paymentData, UserInfo user);

        Task<Result> AuthorizeMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user);

        Task<Result> CaptureMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user);

        Task<Result> VoidMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user);
    }
}