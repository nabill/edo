using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Availabilities
{
    public interface IAccommodationService
    {
        ValueTask<Result<RichAccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode);
    }
}