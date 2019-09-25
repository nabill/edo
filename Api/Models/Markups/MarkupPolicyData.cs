using HappyTravel.Edo.Common.Enums.Markup;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicyData
    {
        [JsonConstructor]
        public MarkupPolicyData(MarkupPolicyTarget target, MarkupPolicySettings settings,
            MarkupPolicyScope scope)
        {
            Target = target;
            Settings = settings;
            Scope = scope;
        }
        
        public MarkupPolicyTarget Target { get; }
        public MarkupPolicySettings Settings { get; }
        public MarkupPolicyScope Scope { get; }
    }
}