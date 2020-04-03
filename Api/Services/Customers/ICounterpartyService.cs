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
        Task<Result<Company>> Add(CounterpartyInfo counterparty);

        Task<Result<CounterpartyInfo>> Get(int companyId);

        Task<Result<CounterpartyInfo>> Update(CounterpartyInfo counterparty, int companyId);

        Task<Result<Branch>> AddBranch(int companyId, BranchInfo branch);

        Task<Result<BranchInfo>> GetBranch(int companyId, int branchId);

        Task<Result<List<BranchInfo>>> GetAllCounterpartyBranches(int companyId);

        Task<Branch> GetDefaultBranch(int companyId);

        Task<Result> VerifyAsFullyAccessed(int companyId, string verificationReason);

        Task<Result> VerifyAsReadOnly(int companyId, string verificationReason);
    }
}