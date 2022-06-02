using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public interface IWideAvailabilitySearchService
    {
        Task<Result<Guid>> StartSearch(AvailabilityRequest request, string languageCode);

        Task<WideAvailabilitySearchState> GetState(Guid searchId);

        Task<IEnumerable<WideAvailabilityResult>> GetResult(Guid searchId, AvailabilitySearchFilter options, string languageCode);
    }
}