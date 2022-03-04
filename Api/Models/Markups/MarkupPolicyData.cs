using HappyTravel.Edo.Common.Enums.Markup;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicyData
    {
        [JsonConstructor]
        public MarkupPolicyData(MarkupPolicySettings settings, MarkupPolicyScope scope)
        {
            Settings = settings;
            Scope = scope;
        }


        /// <summary>
        ///     Policy settings.
        /// </summary>
        public MarkupPolicySettings Settings { get; }

        /// <summary>
        ///     Policy scopes.
        /// </summary>
        public MarkupPolicyScope Scope { get; }
    }
}