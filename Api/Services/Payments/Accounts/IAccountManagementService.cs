using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountManagementService
    {
        Task<Result> CreateForAgency(Agency agency, Currencies currency);

        Task<Result> CreateForCounterparty(Counterparty counterparty, Currencies currency);

        Task<Result> ChangeCreditLimit(int accountId, decimal creditLimit);

        Task<Result<PaymentAccount>> Get(int agencyId, Currencies currency);
    }
}