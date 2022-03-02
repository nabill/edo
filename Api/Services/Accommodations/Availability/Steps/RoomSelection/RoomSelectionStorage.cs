using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionStorage : IRoomSelectionStorage
    {
        public RoomSelectionStorage(IMultiProviderAvailabilityStorage storage)
        {
            _storage = storage;
        }


        public Task SaveResult(Guid searchId, string htId, SingleAccommodationAvailability details, string supplierCode)
        {
            var keyPrefix = BuildKeyPrefix(searchId, htId);
            return _storage.Save(keyPrefix, details, supplierCode);
        }


        public async Task<List<(string SupplierCode, SingleAccommodationAvailability Result)>> GetResult(Guid searchId, string htId, List<string> suppliers)
        {
            var keyPrefix = BuildKeyPrefix(searchId, htId);
            return (await _storage.Get<SingleAccommodationAvailability>(keyPrefix, suppliers))
                .Where(t => !t.Result.Equals(default(SingleAccommodationAvailability)))
                .ToList();
        }


        private string BuildKeyPrefix(Guid searchId, string htId) 
            => $"{searchId}::{htId}";

        private readonly IMultiProviderAvailabilityStorage _storage;
    }
}