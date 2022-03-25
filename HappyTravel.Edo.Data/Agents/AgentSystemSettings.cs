namespace HappyTravel.Edo.Data.Agents
{
    public class AgentSystemSettings
    {
        public int AgentId { get; set; }
        public int AgencyId { get; set; }
        public AgentAccommodationBookingSettings AccommodationBookingSettings { get; set; } = null!;
    }
}