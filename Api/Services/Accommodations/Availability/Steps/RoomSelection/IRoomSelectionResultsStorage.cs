using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public interface IRoomSelectionResultsStorage
    {
        Task SaveResult(Guid searchId, SingleAccommodationAvailabilityDetails details, DataProviders dataProvider);
    }
}