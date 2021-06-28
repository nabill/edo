using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionStorage : IRoomSelectionStorage
    {
        public RoomSelectionStorage(IMultiProviderAvailabilityStorage storage)
        {
            _storage = storage;
        }


        public Task SaveResult(Guid searchId, Guid resultId, SingleAccommodationAvailability details, Suppliers supplier)
        {
            var keyPrefix = BuildKeyPrefix(searchId, resultId);
            return _storage.Save(keyPrefix, details, supplier);
        }


        public async Task<List<(Suppliers Supplier, SingleAccommodationAvailability Result)>> GetResult(Guid searchId, Guid resultId, List<Suppliers> suppliers)
        {
            var keyPrefix = BuildKeyPrefix(searchId, resultId);
            return (await _storage.Get<SingleAccommodationAvailability>(keyPrefix, suppliers))
                .Where(t => !t.Result.Equals(default(SingleAccommodationAvailability)))
                .ToList();
        }


        private string BuildKeyPrefix(Guid searchId, Guid resultId) => $"{searchId}::{resultId}";

        private readonly IMultiProviderAvailabilityStorage _storage;
    }
}