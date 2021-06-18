using HappyTravel.Edo.Api.Models.Settings;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgentSystemSettingsCreateOrEditEventData
    {
        public AgentSystemSettingsCreateOrEditEventData(int agentId, int agencyId, AgentAccommodationBookingSettingsInfo newSettings)
        {
            AgentId = agentId;
            AgencyId = agencyId;
            NewSettings = newSettings;
        }


        public int AgentId { get; }
        public int AgencyId { get; }
        public AgentAccommodationBookingSettingsInfo NewSettings { get; }
    }
}
