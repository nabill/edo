using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupPolicyManagementService
    {
        Task<Result> AddPolicy(MarkupPolicyData policyData);
        Task<Result> DeletePolicy(int policyId);
        Task<Result> UpdatePolicy(int policyId, MarkupPolicySettings settings);
        Task<Result<List<MarkupPolicyData>>> GetGlobalPolicies();
        Task<Result<List<MarkupPolicyData>>> GetCompanyPolicies(int companyId);
        Task<Result<List<MarkupPolicyData>>> GetCustomerPolicies(int customerId);
    }
}