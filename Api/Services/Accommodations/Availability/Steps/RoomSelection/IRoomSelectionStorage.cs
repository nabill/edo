using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public interface IRoomSelectionStorage
    {
        Task SaveResult(Guid searchId, Guid resultId, AccommodationAvailability details, Suppliers supplier);

        Task<List<(Suppliers DataProvider, AccommodationAvailability Result)>> GetResult(Guid searchId, Guid resultId, List<Suppliers> dataProviders);
    }
}