using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public readonly struct AccommodationBookingSettings
    {
        public AccommodationBookingSettings(List<string> enabledConnectors, 
            AprMode aprMode,
            PassedDeadlineOffersMode passedDeadlineOffersMode,
            bool isSupplierVisible,
            CancellationPolicyProcessSettings cancellationPolicyProcessSettings,
            bool isDirectContractFlagVisible,
            SearchFilters additionalSearchFilters)
        {
            CancellationPolicyProcessSettings = cancellationPolicyProcessSettings;
            IsDirectContractFlagVisible = isDirectContractFlagVisible;
            EnabledConnectors = enabledConnectors;
            AprMode = aprMode;
            PassedDeadlineOffersMode = passedDeadlineOffersMode;
            IsSupplierVisible = isSupplierVisible;
            AdditionalSearchFilters = additionalSearchFilters;
        }
        
        public List<string> EnabledConnectors { get; }
        public AprMode AprMode { get; }
        public PassedDeadlineOffersMode PassedDeadlineOffersMode { get; }
        public bool IsSupplierVisible { get; }
        public CancellationPolicyProcessSettings CancellationPolicyProcessSettings { get; }
        public bool IsDirectContractFlagVisible { get; }
        public SearchFilters AdditionalSearchFilters { get; }
    }
}