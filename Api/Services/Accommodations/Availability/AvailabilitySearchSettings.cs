using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AvailabilitySearchSettings
    {
        public AvailabilitySearchSettings(List<DataProviders> enabledConnectors)
        {
            EnabledConnectors = enabledConnectors;
        }
        
        public List<DataProviders> EnabledConnectors { get; }
    }
}