using HappyTravel.Edo.Common.Enums.Markup;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicyScope
    {
        [JsonConstructor]
        public MarkupPolicyScope(MarkupPolicyScopeType type, int? id)
        {
            Type = type;
            Id = id;
        }
        
        /// <summary>
        /// Scope type.
        /// </summary>
        public MarkupPolicyScopeType Type { get; }
        
        /// <summary>
        /// Scope entity Id, can be null for global policies.
        /// </summary>
        public int? Id { get; }
    }
}