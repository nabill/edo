using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AccommodationBookingSettings
    {
        public AccommodationBookingSettings(List<DataProviders> enabledConnectors, 
            AprMode aprMode,
            PassedDeadlineOffersMode passedDeadlineOffersMode,
            bool isMarkupDisabled,
            bool isDataProviderVisible)
        {
            EnabledConnectors = enabledConnectors;
            AprMode = aprMode;
            PassedDeadlineOffersMode = passedDeadlineOffersMode;
            IsMarkupDisabled = isMarkupDisabled;
            IsDataProviderVisible = isDataProviderVisible;
        }
        
        public List<DataProviders> EnabledConnectors { get; }
        public AprMode AprMode { get; }
        public PassedDeadlineOffersMode PassedDeadlineOffersMode { get; }
        public bool IsMarkupDisabled { get; }
        public bool IsDataProviderVisible { get; }
    }
}