using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgencyAccommodationBookingSettings
    {
        public AprMode? AprMode { get; set; }

        public PassedDeadlineOffersMode? PassedDeadlineOffersMode { get; set; }

        public bool IsSupplierVisible { get; set; }

        public bool IsDirectContractFlagVisible { get; set; }

        public int? CustomDeadlineShift { get; set; }

        public List<Currencies> AvailableCurrencies { get; set; }
    }
}