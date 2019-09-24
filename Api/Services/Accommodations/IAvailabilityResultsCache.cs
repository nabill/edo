using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Markups.Availability;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAvailabilityResultsCache
    {
        Task Set(AvailabilityResponseWithMarkup availabilityResponse);
        Task<AvailabilityResponseWithMarkup> Get(int id);
    }
}