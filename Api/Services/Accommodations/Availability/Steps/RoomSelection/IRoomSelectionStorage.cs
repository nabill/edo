using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public interface IRoomSelectionStorage
    {
        Task SaveResult(Guid searchId, string htId, SingleAccommodationAvailability details, string supplierCode);

        Task<List<(string SupplierCode, SingleAccommodationAvailability Result)>> GetResult(Guid searchId, string htId, List<string> suppliers);
    }
}