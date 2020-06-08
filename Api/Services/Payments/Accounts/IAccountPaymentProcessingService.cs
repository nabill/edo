using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountPaymentProcessingService
    {
        Task<Result> AddMoney(int accountId, PaymentData paymentData, UserInfo user);

        Task<Result> AddMoneyToCounterparty(int counterpartyAccountId, PaymentData paymentData, UserInfo user);

        Task<Result> SubtractMoneyFromCounterparty(int counterpartyAccountId, PaymentCancellationData data, UserInfo user);

        Task<Result> ChargeMoney(int accountId, PaymentData paymentData, UserInfo user);

        Task<Result> AuthorizeMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user);

        Task<Result> CaptureMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user);

        Task<Result> VoidMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user);

        Task<Result> TransferToDefaultAgency(int counterpartyAccountId, MoneyAmount amount, UserInfo user);
    }
}