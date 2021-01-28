using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public interface IAccommodationMapperClient
    {
        Task<Result<LocationMapping, ProblemDetails>> GetMapping(string htId, string languageCode);
    }
}