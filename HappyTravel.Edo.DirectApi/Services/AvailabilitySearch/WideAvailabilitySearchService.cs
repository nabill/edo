using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.DirectApi.Models.Search;
using AvailabilityRequest = HappyTravel.Edo.DirectApi.Models.Search.AvailabilityRequest;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    public class WideAvailabilitySearchService
    {
        public WideAvailabilitySearchService(IAccommodationBookingSettingsService accommodationBookingSettingsService, 
            IWideAvailabilitySearchStateStorage stateStorage, IWideAvailabilitySearchService wideAvailabilitySearchService, 
            IWideAvailabilityStorage availabilityStorage)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _stateStorage = stateStorage;
            _wideAvailabilitySearchService = wideAvailabilitySearchService;
            _availabilityStorage = availabilityStorage;
        }


        public async Task<Result<StartSearchResponse>> StartSearch(AvailabilityRequest request)
        {
            var (_, isFailure, searchId, error) = await _wideAvailabilitySearchService.StartSearch(request.ToEdoModel(), "en");

            return isFailure
                ? Result.Failure<StartSearchResponse>(error)
                : new StartSearchResponse(searchId); 
        }


        public async Task<Result<WideAvailabilitySearchResult>> GetResult(Guid searchId)
        {
            var isComplete = await IsComplete(searchId);
            var searchSettings = await _accommodationBookingSettingsService.Get();
            var result = await _availabilityStorage.GetFilteredResults(searchId: searchId, 
                filters: null, 
                searchSettings: searchSettings, 
                suppliers: searchSettings.EnabledConnectors);
            
            return new WideAvailabilitySearchResult(searchId, isComplete, result.MapFromEdoModels());
        }


        private async Task<bool> IsComplete(Guid searchId)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get();
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