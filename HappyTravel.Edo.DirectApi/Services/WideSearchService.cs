using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.DirectApi.Extensions;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.DependencyInjection;
using AvailabilityRequest = HappyTravel.Edo.DirectApi.Models.AvailabilityRequest;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class WideSearchService
    {
        public WideSearchService(IAccommodationBookingSettingsService accommodationBookingSettingsService, 
            IWideAvailabilitySearchStateStorage stateStorage, IWideAvailabilitySearchService wideAvailabilitySearchService, 
            IWideAvailabilityStorage availabilityStorage)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _stateStorage = stateStorage;
            _wideAvailabilitySearchService = wideAvailabilitySearchService;
            _availabilityStorage = availabilityStorage;
        }


        public async Task<Result<StartSearchResponse>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            var (_, isFailure, searchId, error) = await _wideAvailabilitySearchService.StartSearch(request.ToEdoModel(), agent, languageCode);

            return isFailure
                ? Result.Failure<StartSearchResponse>(error)
                : new StartSearchResponse(searchId); 
        }


        public async Task<Result<WideSearchResult>> GetResult(Guid searchId, AgentContext agent)
        {
            var isComplete = await IsComplete(searchId, agent);
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var result = await _availabilityStorage.GetFilteredResults(searchId: searchId, 
                filters: null, 
                searchSettings: searchSettings, 
                suppliers: searchSettings.EnabledConnectors,
                languageCode: "en");
            
            return new WideSearchResult(searchId, isComplete, result.MapFromEdoModels());
        }


         private async Task<bool> IsComplete(Guid searchId, AgentContext agent)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var searchStates = await _stateStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            var state = WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);

            return state.TaskState is not AvailabilitySearchTaskState.Pending or AvailabilitySearchTaskState.PartiallyCompleted;
        }

        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly IWideAvailabilitySearchService _wideAvailabilitySearchService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
    }
}