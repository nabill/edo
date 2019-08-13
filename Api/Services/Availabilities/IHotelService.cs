using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Hotels;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Availabilities
{
    public interface IHotelService
    {
        ValueTask<Result<RichHotelDetails, ProblemDetails>> Get(string hotelId, string languageCode);
    }
}