using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountPaymentService
    {
        Task<Result> ReplenishAccount(int accountId, PaymentData payment, Administrator administrator);
        Task<bool> CanPayWithAccount(AgentInfo agentInfo);
        Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency);
        Task<Result<string>> CaptureMoney(Booking booking, UserInfo user);
        Task<Result<PaymentResponse>> AuthorizeMoney(AccountBookingPaymentRequest request, AgentInfo agentInfo, string ipAddress);
        Task<Result> VoidMoney(Booking booking, UserInfo user);
        Task<Result<Price>> GetPendingAmount(Booking booking);
        Task<Result<CounterpartyBalanceInfo>> GetCounterpartyBalance(int counterpartyId, Currencies currency);
        Task<Result> ReplenishCounterpartyAccount(int counterpartyAccountId, PaymentData payment, Administrator administrator);
        Task<Result> SubtractMoneyCounterparty(int counterpartyAccountId, PaymentCancellationData data, Administrator administrator);
        Task<Result> TransferToDefaultAgency(int counterpartyAccountId, TransferData transferData, Administrator administrator);
    }
}