using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AvailabilitySearchSettings
    {
        public AvailabilitySearchSettings(List<DataProviders> enabledConnectors, AprMode aprMode)
        {
            EnabledConnectors = enabledConnectors;
            AprMode = aprMode;
        }
        
        public List<DataProviders> EnabledConnectors { get; }
        public AprMode AprMode { get; }
    }
}