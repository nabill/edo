using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Internal.Mappings;
using HappyTravel.MapperContracts.Public.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public interface IAccommodationMapperClient
    {
        Task<Result<List<LocationMapping>, ProblemDetails>> GetMappings(List<string> htIds, string languageCode);
        Task<List<SlimAccommodation>> GetAccommodations(List<string> htIds, string languageCode);
    }
}