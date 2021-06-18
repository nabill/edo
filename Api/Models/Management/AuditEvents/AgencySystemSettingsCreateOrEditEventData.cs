using HappyTravel.Edo.Api.Models.Settings;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgencySystemSettingsCreateOrEditEventData
    {
        public AgencySystemSettingsCreateOrEditEventData(int agencyId, AgencyAccommodationBookingSettingsInfo newSettings)
        {
            AgencyId = agencyId;
            NewSettings = newSettings;
        }


        public int AgencyId { get; }
        public AgencyAccommodationBookingSettingsInfo NewSettings { get; }
    }
}
