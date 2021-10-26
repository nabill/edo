using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface ICounterpartySystemSettingsService
    {
        Task<CounterpartyAccommodationBookingSettings> GetAccommodationBookingSettings(int agentAgencyId);
    }
}