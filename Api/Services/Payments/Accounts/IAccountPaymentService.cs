using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountPaymentService
    {
        Task<Result> ReplenishAccount(int accountId, PaymentData payment);
        Task<bool> CanPayWithAccount(AgentInfo agentInfo);
        Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency);
        Task<Result<string>> CaptureMoney(Booking booking);
        Task<Result<PaymentResponse>> AuthorizeMoney(AccountBookingPaymentRequest request, AgentInfo agentInfo, string ipAddress);
        Task<Result> VoidMoney(Booking booking);
        Task<Result<Price>> GetPendingAmount(Booking booking);
    }
}