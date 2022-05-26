using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgentSystemSettings
    {
        public int AgentId { get; set; }
        public int AgencyId { get; set; }
        public AgentAccommodationBookingSettings? AccommodationBookingSettings { get; set; }
        public Dictionary<string, bool>? EnabledSuppliers { get; set; }
    }
}