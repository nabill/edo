using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public interface IBookingRegistrationService
    {
        Task<Result<Booking>> Register(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo availabilityInfo, PaymentTypes paymentMethod,
            string languageCode);
    }
}