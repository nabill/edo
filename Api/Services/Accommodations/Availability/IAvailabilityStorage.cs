using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IAvailabilityStorage
    {
        Task<(DataProviders DataProvider, TObject Result)[]> GetProviderResults<TObject>(Guid searchId);

        Task SaveObject<TObjectType>(Guid searchId, TObjectType @object, DataProviders? dataProvider = null);
    }
}