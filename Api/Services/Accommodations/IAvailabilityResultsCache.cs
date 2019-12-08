using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Markups.Availability;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAvailabilityResultsCache
    {
        Task Set(SingleAccommodationAvailabilityDetailsWithMarkup availabilityResponse);
        Task<SingleAccommodationAvailabilityDetailsWithMarkup> Get(int id);
    }
}