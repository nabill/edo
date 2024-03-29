﻿using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Models.Settings
{
    public readonly struct AgentAccommodationBookingSettingsInfo
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
        /// These filters apply to existing search filters during availability search
        /// </summary>
        public SearchFilters AdditionalSearchFilters { get; init; }

        /// <summary>
        /// Custom deadline shift for an agent
        /// </summary>
        public int? CustomDeadlineShift { get; init; }
    }
}
