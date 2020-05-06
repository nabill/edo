using HappyTravel.Edo.Common.Enums.Markup;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicyScope
    {
        [JsonConstructor]
        public MarkupPolicyScope(MarkupPolicyScopeType type, int? scopeId = null)
        {
            Type = type;
            ScopeId = scopeId;
        }


        public void Deconstruct(out MarkupPolicyScopeType type, out int? counterpartyId, out int? agencyId, out int? agentId)
        {
            type = Type;
            counterpartyId = null;
            agencyId = null;
            agentId = null;

            switch (type)
            {
                case MarkupPolicyScopeType.Counterparty:
                    counterpartyId = ScopeId;
                    break;
                case MarkupPolicyScopeType.Agency:
                    agencyId = ScopeId;
                    break;
                case MarkupPolicyScopeType.Agent:
                    agentId = ScopeId;
                    break;
            }
        }


        /// <summary>
        ///     Scope type.
        /// </summary>
        public MarkupPolicyScopeType Type { get; }

        /// <summary>
        ///     Scope entity Id, can be null for global policies.
        /// </summary>
        public int? ScopeId { get; }
    }
}