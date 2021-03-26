using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AccommodationBookingSettings
    {
        public AccommodationBookingSettings(List<Suppliers> enabledConnectors, 
            AprMode aprMode,
            PassedDeadlineOffersMode passedDeadlineOffersMode,
            bool isMarkupDisabled,
            bool isSupplierVisible,
            CancellationPolicyProcessSettings cancellationPolicyProcessSettings,
            bool areTagsVisible,
            SearchFilters defaultSearchFilters)
        {
            CancellationPolicyProcessSettings = cancellationPolicyProcessSettings;
            AreTagsVisible = areTagsVisible;
            EnabledConnectors = enabledConnectors;
            AprMode = aprMode;
            PassedDeadlineOffersMode = passedDeadlineOffersMode;
            IsMarkupDisabled = isMarkupDisabled;
            IsSupplierVisible = isSupplierVisible;
            DefaultSearchFilters = defaultSearchFilters;
        }
        
        public List<Suppliers> EnabledConnectors { get; }
        public AprMode AprMode { get; }
        public PassedDeadlineOffersMode PassedDeadlineOffersMode { get; }
        public bool IsMarkupDisabled { get; }
        public bool IsSupplierVisible { get; }
        public CancellationPolicyProcessSettings CancellationPolicyProcessSettings { get; }
        public bool AreTagsVisible { get; }
        public SearchFilters DefaultSearchFilters { get; }
    }
}