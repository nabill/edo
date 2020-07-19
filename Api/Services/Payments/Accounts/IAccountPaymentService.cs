using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountPaymentService
    {
        Task<bool> CanPayWithAccount(AgentContext agentContext);
        Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency);
        Task<Result<string>> CaptureMoney(Booking booking, UserInfo user);
        Task<Result<PaymentResponse>> AuthorizeMoney(AccountBookingPaymentRequest request, AgentContext agentContext, string ipAddress);
        Task<Result> VoidMoney(Booking booking, UserInfo user);
        Task<Result<Price>> GetPendingAmount(Booking booking);
        Task<Result> TransferToChildAgency(int payerAccountId, int recipientAccountId, MoneyAmount amount);
    }
}