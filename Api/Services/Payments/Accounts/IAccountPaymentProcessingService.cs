using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountPaymentProcessingService
    {
        Task<Result> ChargeMoney(int accountId, ChargedMoneyData paymentData);

        Task<Result> RefundMoney(int accountId, ChargedMoneyData paymentData);

        Task<Result> TransferToChildAgency(int payerAccountId, int recipientAccountId, MoneyAmount amount);
    }
}