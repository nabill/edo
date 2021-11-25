using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IRootAgencySystemSettingsService
    {
        Task<RootAgencyAccommodationBookingSettings> GetAccommodationBookingSettings(int agentAgencyId);
    }
}