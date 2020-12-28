using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountPaymentService
    {
        Task<bool> CanPayWithAccount(AgentContext agentContext);
        Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, AgentContext agent);
        Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, int agencyId);
        Task<Result<PaymentResponse>> Charge(string referenceCode, UserInfo user, IPaymentsService paymentsService);
        Task<Result> Refund(string referenceCode, MoneyAmount refundableAmount, UserInfo user, IPaymentsService paymentsService, string reason);
        Task<Result> TransferToChildAgency(int payerAccountId, int recipientAccountId, MoneyAmount amount, AgentContext agent);
    }
}