using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.DirectApi.Models.Search;
using AvailabilityRequest = HappyTravel.Edo.DirectApi.Models.Search.AvailabilityRequest;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    public class WideAvailabilitySearchService
    {
        public WideAvailabilitySearchService(IWideAvailabilitySearchService wideAvailabilitySearchService)
        {
            _wideAvailabilitySearchService = wideAvailabilitySearchService;
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
            var result =  await _wideAvailabilitySearchService.GetResult(searchId, null, "en");
            return new WideAvailabilitySearchResult(searchId, isComplete, result.ToList().MapFromEdoModels());
        }


        private async Task<bool> IsComplete(Guid searchId)
        {
            var state = await _wideAvailabilitySearchService.GetState(searchId);
            return state.TaskState is not AvailabilitySearchTaskState.Pending or AvailabilitySearchTaskState.PartiallyCompleted;
        }

        private readonly IWideAvailabilitySearchService _wideAvailabilitySearchService;
    }
}