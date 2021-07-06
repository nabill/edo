using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Hotels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Hotel
{
    public class HotelConfirmationService : IHotelConfirmationService
    {
        public async Task<Result> Update(HotelConfirmation hotelConfirmation)
        {
            return Result.Success();
        }
    }
}
