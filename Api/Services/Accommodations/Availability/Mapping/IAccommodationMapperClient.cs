using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.MapperContracts.Internal.Mappings;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public interface IAccommodationMapperClient
    {
        Task<Result<List<LocationMapping>, ProblemDetails>> GetMappings(List<string> htIds, string languageCode, CancellationToken cancellationToken = default);
        Task<Result<SlimLocationDescription,ProblemDetails>> GetSlimLocationDescription(string htId, CancellationToken cancellationToken = default);
        Task<Result<LocalityInfo, ProblemDetails>> GetLocalityInfo(string htId, CancellationToken cancellationToken = default);
        Task<List<SlimAccommodation>> GetAccommodations(List<string> htIds, string languageCode, CancellationToken cancellationToken = default);
        Task<Result<Accommodation, ProblemDetails>> GetAccommodation(string htId, string languageCode, CancellationToken cancellationToken = default);
        Task<Result<Accommodation, ProblemDetails>> GetAccommodation(string supplierCode, string accommodationId, string languageCode, CancellationToken cancellationToken = default);
        Task<Result<List<string>, ProblemDetails>> GetAccommodationEmails(string htId, CancellationToken cancellationToken = default);
        Task<List<string>> FilterHtIdsByRating(List<string> htIds, List<AccommodationRatings> ratings, CancellationToken cancellationToken = default);
    }
}