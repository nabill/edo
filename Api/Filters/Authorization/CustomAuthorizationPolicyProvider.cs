using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using HappyTravel.Edo.Common.Enums.Administrators;

namespace HappyTravel.Edo.Api.Filters.Authorization
{
    public class CustomAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }


        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.Equals(AgentRequiredAttribute.PolicyName))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new AgentRequiredAuthorizationRequirement())
                    .Build());
            }
            
            if (policyName.Equals(ServiceAccountRequiredAttribute.PolicyName))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new ServiceAccountRequiredAuthorizationRequirement())
                    .Build());
            }
            
            if (policyName.StartsWith(InAgencyPermissionsAttribute.PolicyPrefix) 
                && Enum.TryParse(policyName.Substring(InAgencyPermissionsAttribute.PolicyPrefix.Length), out InAgencyPermissions permissions))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new InAgencyPermissionsAuthorizationRequirement(permissions))
                    .Build());
            }

            if (policyName.StartsWith(MinAgencyVerificationStateAttribute.PolicyPrefix)
                && Enum.TryParse(policyName.Substring(MinAgencyVerificationStateAttribute.PolicyPrefix.Length), out AgencyVerificationStates agencyVerificationState))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new MinAgencyVerificationStateAuthorizationRequirement(agencyVerificationState))
                    .Build());
            }
            
            if (policyName.StartsWith(AdministratorPermissionsAttribute.PolicyPrefix)
                && Enum.TryParse(policyName.Substring(AdministratorPermissionsAttribute.PolicyPrefix.Length), out AdministratorPermissions administratorPermissions))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new AdministratorPermissionsAuthorizationRequirement(administratorPermissions))
                    .Build());
            }

            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
        
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();
        
        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
        
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    }
}