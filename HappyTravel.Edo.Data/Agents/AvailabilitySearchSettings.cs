using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Data.Agents
{
    public readonly struct AvailabilitySearchSettings
    {
        [JsonConstructor]
        public AvailabilitySearchSettings(List<DataProviders> enabledProviders)
        {
            EnabledProviders = enabledProviders ?? new List<DataProviders>();
        }
        
        public List<DataProviders> EnabledProviders { get; }
    }
}