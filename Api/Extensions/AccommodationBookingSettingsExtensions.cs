using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AccommodationBookingSettingsExtensions
    {
        public static AgencyAccommodationBookingSettingsInfo ToAgencyAccommodationBookingSettingsInfo(this AgencyAccommodationBookingSettings settings)
            => new ()
            {
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToBoolDictionary(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                CustomDeadlineShift = settings.CustomDeadlineShift
            };


        public static AgentAccommodationBookingSettingsInfo ToAgentAccommodationBookingSettingsInfo(this AgentAccommodationBookingSettings settings)
            => new ()
            {
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToBoolDictionary(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                AdditionalSearchFilters = settings.AdditionalSearchFilters
            };


        public static AgencyAccommodationBookingSettings ToAgencyAccommodationBookingSettings(this AgencyAccommodationBookingSettingsInfo settings)
            => new ()
            {
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToList(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                CustomDeadlineShift = settings.CustomDeadlineShift
            };


        public static AgentAccommodationBookingSettings ToAgentAccommodationBookingSettings(this AgentAccommodationBookingSettingsInfo settings)
            => new ()
            {
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToList(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                AdditionalSearchFilters = settings.AdditionalSearchFilters
            };
        

        public static Dictionary<int, bool> ToBoolDictionary(this List<int> suppliers)
        {
            if (suppliers == null)
                return null;

            var suppliersMap = new Dictionary<int, bool>();

            foreach (var possibleSupplier in suppliers.Except(SuppliersToHide))
                suppliersMap[possibleSupplier] = false;

            foreach (var actualSupplier in suppliers)
                suppliersMap[actualSupplier] = true;

            return suppliersMap;
        }


        private static List<int> ToList(this Dictionary<int, bool> suppliersMap)
            => suppliersMap?
                .Where(p => p.Value)
                .Select(p => p.Key)
                .ToList();


        private static readonly List<int> SuppliersToHide = new() {0};
    }
}
