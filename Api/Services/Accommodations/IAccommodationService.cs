using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAccommodationService
    {
        ValueTask<Result<AccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode);
    }
}