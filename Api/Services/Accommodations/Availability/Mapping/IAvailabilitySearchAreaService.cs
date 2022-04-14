using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public interface IAvailabilitySearchAreaService
    {
        Task<Result<SearchArea>> GetSearchArea(List<string> htIds, string languageCode, CancellationToken cancellationToken = default);
    }
}