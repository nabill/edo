using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups.Availability;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAvailabilityResultsCache
    {
        Task Set(SingleAccommodationAvailabilityDetailsWithMarkup availabilityResponse);

        Task<Result<SingleAccommodationAvailabilityDetailsWithMarkup>> Get(long id);
    }
}