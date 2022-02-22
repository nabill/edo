using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.DirectApi.Models.Search;
using HappyTravel.SupplierOptionsProvider;
using AvailabilityRequest = HappyTravel.Edo.DirectApi.Models.Search.AvailabilityRequest;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    public class WideAvailabilitySearchService
    {
        public WideAvailabilitySearchService(IAccommodationBookingSettingsService accommodationBookingSettingsService, 
            IWideAvailabilitySearchStateStorage stateStorage, IWideAvailabilitySearchService wideAvailabilitySearchService, 
            IWideAvailabilityStorage availabilityStorage, ISupplierOptionsStorage supplierOptionsStorage)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _stateStorage = stateStorage;
            _wideAvailabilitySearchService = wideAvailabilitySearchService;
            _availabilityStorage = availabilityStorage;
            _supplierOptionsStorage = supplierOptionsStorage;
        }


        public async Task<Result<StartSearchResponse>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            var (_, isFailure, searchId, error) = await _wideAvailabilitySearchService.StartSearch(request.ToEdoModel(), agent, languageCode);

            return isFailure
                ? Result.Failure<StartSearchResponse>(error)
                : new StartSearchResponse(searchId); 
        }


        public async Task<Result<WideAvailabilitySearchResult>> GetResult(Guid searchId, AgentContext agent, string languageCode)
        {
            var isComplete = await IsComplete(searchId, agent);
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var supplierIds = searchSettings.EnabledConnectors.Select(s => _supplierOptionsStorage.GetByCode(s).Id).ToList();
            var result = await _availabilityStorage.GetFilteredResults(searchId: searchId, 
                filters: null, 
                searchSettings: searchSettings, 
                suppliers: supplierIds,
                languageCode: languageCode);
            
            return new WideAvailabilitySearchResult(searchId, isComplete, result.MapFromEdoModels());
        }


        private async Task<bool> IsComplete(Guid searchId, AgentContext agent)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var supplierIds = searchSettings.EnabledConnectors.Select(s => _supplierOptionsStorage.GetByCode(s).Id).ToList();
            var searchStates = await _stateStorage.GetStates(searchId, supplierIds);
            var state = WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);

            return state.TaskState is not AvailabilitySearchTaskState.Pending or AvailabilitySearchTaskState.PartiallyCompleted;
        }

        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly IWideAvailabilitySearchService _wideAvailabilitySearchService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}