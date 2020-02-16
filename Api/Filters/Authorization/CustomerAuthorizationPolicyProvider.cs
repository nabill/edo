using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.CompanyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InCompanyPermissionFilters;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Filters.Authorization
{
    public class CustomerAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        public CustomerAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }


        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(InCompanyPermissionsAttribute.PolicyPrefix) 
                && Enum.TryParse(policyName.Substring(InCompanyPermissionsAttribute.PolicyPrefix.Length), out InCompanyPermissions permissions))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new InCompanyPermissionsAuthorizationRequirement(permissions))
                    .Build());
            }

            if (policyName.StartsWith(MinCompanyStateAttribute.PolicyPrefix)
                && Enum.TryParse(policyName.Substring(MinCompanyStateAttribute.PolicyPrefix.Length), out CompanyStates companyState))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new MinCompanyStateAuthorizationRequirement(companyState))
                    .Build());
            }

            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
        
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();
        
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    }
}