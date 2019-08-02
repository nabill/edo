using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Availabilities
{
    public interface IAvailabilityService
    {
        ValueTask<Result<AvailabilityResponse, ProblemDetails>> Get(AvailabilityRequest request, string languageCode);
    }
}