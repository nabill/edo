using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Settings
{
    public readonly struct AgencyAccommodationBookingSettingsInfo
    {
        /// <summary>
        /// Tells whether the user can see and book accommodations with advanced purchase flag
        /// </summary>
        public AprMode? AprMode { get; init; }

        /// <summary>
        /// Tells whether the user can see and book accommodations with passed deadline date
        /// </summary>
        public PassedDeadlineOffersMode? PassedDeadlineOffersMode { get; init; }

        /// <summary>
        /// The user will see suppliers in search UI if this setting is set to true
        /// </summary>
        public bool IsSupplierVisible { get; init; }

        /// <summary>
        /// The user will see the DirectContractFlag if this setting is set to true
        /// </summary>
        public bool IsDirectContractFlagVisible { get; init; }

        /// <summary>
        /// Amount of days, on which deadline policies dated will be shifted towards past. Must not be a positive number
        /// </summary>
        public int? CustomDeadlineShift { get; init; }

        /// <summary>
        /// Existing available currencies for the specific agency
        /// </summary>
        public List<Currencies>? AvailableCurrencies { get; init; }
    }
}
