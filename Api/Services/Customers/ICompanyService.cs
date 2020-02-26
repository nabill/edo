using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Branches;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICompanyService
    {
        Task<Result<Company>> Add(CompanyInfo company);

        Task<Result<CompanyInfo>> Get(int companyId);

        Task<Result<CompanyInfo>> Update(CompanyInfo company, int companyId);

        Task<Result<Branch>> AddBranch(int companyId, BranchInfo branch);

        Task<Result<BranchInfo>> GetBranch(int companyId, int branchId);

        Task<Result<List<BranchInfo>>> GetAllCompanyBranches(int companyId);

        Task<Branch> GetDefaultBranch(int companyId);

        Task<Result> VerifyAsFullyAccessed(int companyId, string verificationReason);

        Task<Result> VerifyAsReadOnly(int companyId, string verificationReason);
    }
}