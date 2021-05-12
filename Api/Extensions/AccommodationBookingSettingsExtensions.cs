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
            => new AgencyAccommodationBookingSettingsInfo
            {
                IsMarkupDisabled = settings.IsMarkupDisabled,
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToBoolDictionary(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                CustomDeadlineShift = settings.CustomDeadlineShift
            };


        public static AgentAccommodationBookingSettingsInfo ToAgentAccommodationBookingSettingsInfo(this AgentAccommodationBookingSettings settings)
            => new AgentAccommodationBookingSettingsInfo
            {
                IsMarkupDisabled = settings.IsMarkupDisabled,
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToBoolDictionary(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                AdditionalSearchFilters = settings.AdditionalSearchFilters
            };


        public static AgencyAccommodationBookingSettings ToAgencyAccommodationBookingSettings(this AgencyAccommodationBookingSettingsInfo settings)
            => new AgencyAccommodationBookingSettings
            {
                IsMarkupDisabled = settings.IsMarkupDisabled,
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToEnumList(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                CustomDeadlineShift = settings.CustomDeadlineShift
            };


        public static AgentAccommodationBookingSettings ToAgentAccommodationBookingSettings(this AgentAccommodationBookingSettingsInfo settings)
            => new AgentAccommodationBookingSettings
            {
                IsMarkupDisabled = settings.IsMarkupDisabled,
                AprMode = settings.AprMode,
                PassedDeadlineOffersMode = settings.PassedDeadlineOffersMode,
                EnabledSuppliers = settings.EnabledSuppliers.ToEnumList(),
                IsSupplierVisible = settings.IsSupplierVisible,
                IsDirectContractFlagVisible = settings.IsDirectContractFlagVisible,
                AdditionalSearchFilters = settings.AdditionalSearchFilters
            };
        

        public static Dictionary<Common.Enums.Suppliers, bool> ToBoolDictionary(this List<Common.Enums.Suppliers> suppliers)
        {
            if (suppliers == null)
                return null;

            var suppliersMap = new Dictionary<Common.Enums.Suppliers, bool>();

            foreach (var possibleSupplier in Enum.GetValues<Common.Enums.Suppliers>().Except(SuppliersToHide))
                suppliersMap[possibleSupplier] = false;

            foreach (var actualSupplier in suppliers)
                suppliersMap[actualSupplier] = true;

            return suppliersMap;
        }


        private static List<Common.Enums.Suppliers> ToEnumList(this Dictionary<Common.Enums.Suppliers, bool> suppliersMap)
            => suppliersMap?
                .Where(p => p.Value)
                .Select(p => p.Key)
                .ToList();


        private static readonly List<Common.Enums.Suppliers> SuppliersToHide = new List<Common.Enums.Suppliers>() {Common.Enums.Suppliers.Unknown};
    }
}
