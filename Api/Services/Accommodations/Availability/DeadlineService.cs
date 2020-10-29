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
        public DeadlineService(IWideAvailabilityStorage availabilityStorage, 
            IRoomSelectionStorage roomSelectionStorage,
            IDataProviderManager dataProviderManager,
            IAccommodationBookingSettingsService accommodationBookingSettingsService)
        {
            _availabilityStorage = availabilityStorage;
            _roomSelectionStorage = roomSelectionStorage;
            _dataProviderManager = dataProviderManager;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
        }


        public async Task<Result<Deadline, ProblemDetails>> GetDeadlineDetails(Guid searchId, Guid resultId, Guid roomContractSetId, AgentContext agent,
            string languageCode)
        {
            var enabledProviders = (await _accommodationBookingSettingsService.Get(agent)).EnabledConnectors;
            var (_, isFailure, result, _) = await GetDeadlineByWideAvailabilitySearchStorage();
            // This request can be from first and second step, that is why we check two caches.
            return isFailure ? await GetDeadlineByRoomSelectionStorage() : result;


            async Task<Result<Deadline, ProblemDetails>> GetDeadlineByRoomSelectionStorage()
            {
                var selectedRoomSet = (await _roomSelectionStorage.GetResult(searchId, resultId, enabledProviders))
                    .SelectMany(r =>
                    {
                        return r.Result.RoomContractSets
                            .Select(rs => (Provider: r.DataProvider, RoomContractSetId: rs.Id, AvailabilityId: r.Result.AvailabilityId));
                    })
                    .SingleOrDefault(r => r.RoomContractSetId == roomContractSetId);

                if (selectedRoomSet.Equals(default))
                    return ProblemDetailsBuilder.Fail<Deadline>("Could not find selected room contract set");

                return await MakeProviderRequest(selectedRoomSet.Provider, selectedRoomSet.RoomContractSetId, selectedRoomSet.AvailabilityId);
            }


            async Task<Result<Deadline, ProblemDetails>> GetDeadlineByWideAvailabilitySearchStorage()
            {
                var selectedResult = (await _availabilityStorage.GetResults(searchId, enabledProviders))
                    .SelectMany(r => r.AccommodationAvailabilities.Select(a => (r.ProviderKey, a)))
                    .SingleOrDefault(r => r.a.Id == resultId);
                
                var selectedRoom = selectedResult.a.RoomContractSets?.SingleOrDefault(r => r.Id == roomContractSetId);

                if (selectedRoom is null || selectedRoom.Value.Equals(default))
                    return ProblemDetailsBuilder.Fail<Deadline>("Could not find selected room contract set");

                return await MakeProviderRequest(selectedResult.ProviderKey, selectedRoom.Value.Id, selectedResult.a.AvailabilityId);
            }


            Task<Result<Deadline, ProblemDetails>> MakeProviderRequest(Suppliers provider, Guid roomSetId, string availabilityId)
                => _dataProviderManager.Get(provider)
                    .GetDeadline(availabilityId, roomSetId, languageCode);
        }


        private readonly IDataProviderManager _dataProviderManager;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
    }
}