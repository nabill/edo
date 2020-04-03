using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Branches;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICounterpartyService
    {
        Task<Result<Counterparty>> Add(CounterpartyInfo counterparty);

        Task<Result<CounterpartyInfo>> Get(int counterpartyId);

        Task<Result<CounterpartyInfo>> Update(CounterpartyInfo counterparty, int counterpartyId);

        Task<Result<Branch>> AddBranch(int counterpartyId, BranchInfo branch);

        Task<Result<BranchInfo>> GetBranch(int counterpartyId, int branchId);

        Task<Result<List<BranchInfo>>> GetAllCounterpartyBranches(int counterpartyId);

        Task<Branch> GetDefaultBranch(int counterpartyId);

        Task<Result> VerifyAsFullyAccessed(int counterpartyId, string verificationReason);

        Task<Result> VerifyAsReadOnly(int counterpartyId, string verificationReason);
    }
}