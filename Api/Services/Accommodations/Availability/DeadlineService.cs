using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class DeadlineService : IDeadlineService
    {
        public DeadlineService(IWideAvailabilityStorage availabilityStorage, IRoomSelectionStorage roomSelectionStorage,
            IDataProviderManager dataProviderManager)
        {
            _availabilityStorage = availabilityStorage;
            _roomSelectionStorage = roomSelectionStorage;
            _dataProviderManager = dataProviderManager;
        }


        public async Task<Result<Deadline, ProblemDetails>> GetDeadlineDetails(Guid searchId, Guid resultId, Guid roomContractSetId, AgentContext agent,
            string languageCode)
        {
            var enabledProviders = await _dataProviderManager.GetEnabled(agent);
            
            var selectedResult = (await _availabilityStorage.GetResults(searchId, enabledProviders))
                .SelectMany(r => r.AccommodationAvailabilities.Select(a => (r.ProviderKey, a)))
                .SingleOrDefault(r => r.a.Id == resultId);
            var selectedProvider = selectedResult.ProviderKey;
            var selectedRoom = selectedResult.a.RoomContractSets?.SingleOrDefault(r => r.Id == roomContractSetId);
            
            // This request can be from first and second step, that is why we check two caches.
            if (selectedRoom is null || selectedRoom.Value.Equals(default))
            {
                var providerRoomContractSets = (await _roomSelectionStorage.GetResult(searchId, resultId, await _dataProviderManager.GetEnabled(agent)))
                    .SingleOrDefault(r => r.Result.RoomContractSets.Any(rc => rc.Id == roomContractSetId));
                selectedProvider = providerRoomContractSets.DataProvider;

                if (selectedProvider == DataProviders.Unknown)
                    return ProblemDetailsBuilder.Fail<Deadline>("Could not find selected availability result");

                selectedRoom = providerRoomContractSets.Result.RoomContractSets.SingleOrDefault(r => r.Id == roomContractSetId);
            }

            return await _dataProviderManager.Get(selectedProvider)
                .GetDeadline(selectedResult.a.AvailabilityId, selectedRoom.Value.Id, languageCode);
        }


        private readonly IDataProviderManager _dataProviderManager;
        private readonly IWideAvailabilityStorage _availabilityStorage;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
    }
}