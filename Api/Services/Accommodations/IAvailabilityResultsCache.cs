using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAvailabilityResultsCache
    {
        Task Set(DataProviders dataProvider, DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline> availabilityResponse);

        Task<Result<DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline> >> Get(DataProviders dataProvider, string id);
    }
}