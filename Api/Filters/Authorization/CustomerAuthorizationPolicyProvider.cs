using System;
using System.Threading.Tasks;
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
            if (policyName.StartsWith(PolicyPrefix) && Enum.TryParse(policyName.Substring(PolicyPrefix.Length), out InCompanyPermissions permissions))
            {
                return Task.FromResult(new AuthorizationPolicyBuilder()
                    .AddRequirements(new InCompanyPermissionsAuthorizationRequirement(permissions))
                    .Build());
            }

            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }


        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public const string PolicyPrefix = "Customer_";
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    }
}