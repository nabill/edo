using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IAccommodationBookingSettingsService
    {
        Task<AccommodationBookingSettings> Get();
    }
}