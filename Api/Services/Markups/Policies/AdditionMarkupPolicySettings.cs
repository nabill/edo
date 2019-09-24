using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Markups.Policies
{
    public readonly struct AdditionMarkupPolicySettings
    {
        [JsonConstructor]
        public AdditionMarkupPolicySettings(decimal addition, Currencies currency)
        {
            Addition = addition;
            Currency = currency;
        }
        
        public decimal Addition { get; }
        public Currencies Currency { get; }
    }
}