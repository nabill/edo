namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct BookingAgentInformation
    {
        public BookingAgentInformation(string agentName, string agencyName, string agentEmail)
        {
            AgentName = agentName;
            AgencyName = agencyName;
            AgentEmail = agentEmail;
        }
        public string AgentName { get; }
        public string AgencyName { get; }
        public string AgentEmail { get; }
    }
}
