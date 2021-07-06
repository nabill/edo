using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Hotels;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Hotel
{
    public interface IHotelConfirmationService
    {
        Task<Result> Update(HotelConfirmation hotelConfirmation);
    }
}
