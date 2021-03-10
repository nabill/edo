using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public interface IRoomSelectionStorage
    {
        Task SaveResult(Guid searchId, Guid resultId, SingleAccommodationAvailability details, Suppliers supplier);

        Task<List<(Suppliers Supplier, SingleAccommodationAvailability Result)>> GetResult(Guid searchId, Guid resultId, List<Suppliers> suppliers);
    }
}