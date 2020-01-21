using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IBookingRequestDataLogService
    {
        Task<Result<BookingRequestDataEntry>> Get(string referenceCode);
        Task<Result> Add(string bookingReferenceCode, AccommodationBookingRequest bookingRequest, string languageCode, DataProviders dataProvider);
    }
}