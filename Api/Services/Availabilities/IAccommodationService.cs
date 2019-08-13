using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Hotels;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Availabilities
{
    public interface IAccommodationService
    {
        ValueTask<Result<RichHotelDetails, ProblemDetails>> Get(string accommodationId, string languageCode);
    }
}