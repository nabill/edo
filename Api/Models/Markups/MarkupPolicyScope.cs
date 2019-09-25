using HappyTravel.Edo.Common.Enums.Markup;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicyScope
    {
        [JsonConstructor]
        public MarkupPolicyScope(MarkupPolicyScopeType type, int? companyId, int? branchId, int? customerId)
        {
            Type = type;
            CompanyId = companyId;
            BranchId = branchId;
            CustomerId = customerId;
        }
        
        /// <summary>
        /// Scope type.
        /// </summary>
        public MarkupPolicyScopeType Type { get; }
        
        /// <summary>
        /// Scope entity Id, can be null for global policies.
        /// </summary>
        public int? CompanyId { get; }
        
        public int? BranchId { get; }
        
        public int? CustomerId { get; }
    }
}