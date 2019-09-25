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
        
        public MarkupPolicyScopeType Type { get; }
        public int? Id { get; }
    }
}