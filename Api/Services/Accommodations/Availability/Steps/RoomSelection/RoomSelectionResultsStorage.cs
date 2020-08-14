using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionResultsStorage : IRoomSelectionResultsStorage
    {
        public RoomSelectionResultsStorage(IAvailabilityStorage storage)
        {
            _storage = storage;
        }


        public Task SaveResult(Guid searchId, SingleAccommodationAvailabilityDetails details, DataProviders dataProvider)
        {
            return _storage.SaveObject(searchId, details, dataProvider);
        }
        
        private readonly IAvailabilityStorage _storage;
    }
}