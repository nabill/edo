using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IBookingResponseDataLogService
    {
        Task<Result> Add(BookingDetails bookingDetails);
    }
}