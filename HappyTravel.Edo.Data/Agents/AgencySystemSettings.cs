using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgencySystemSettings
    {
        public int AgencyId { get; set; }

        public AgencyAccommodationBookingSettings? AccommodationBookingSettings { get; set; }

        public Dictionary<string, bool> EnabledSuppliers { get; set; } = new();
    }
}