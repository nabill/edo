using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Converters.EnumConverters;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingDocumentsService : IBookingDocumentsService
    {
        public BookingDocumentsService(IBookingManager bookingManager, IAccommodationService accommodationService)
        {
            _accommodationService = accommodationService;
            _bookingManager = bookingManager;
        }


        public Task<Result<BookingVoucherData>> GenerateVoucher(int bookingId, string languageCode)
        {
            return GetBookingData(bookingId)
                .OnSuccess(CreateVoucherData);


            async Task<Result<BookingVoucherData>> CreateVoucherData(
                (AccommodationBookingInfo bookingInfo, BookingAvailabilityInfo serviceDetails, AccommodationBookingDetails bookingDetails) bookingData)
            {
                var serviceDetails = bookingData.serviceDetails;
                var bookingDetails = bookingData.bookingDetails;
                var bookingInfo = bookingData.bookingInfo;

                var (_, isBookingFailure, booking, bookingError) = await _bookingManager.Get(bookingId);
                if (isBookingFailure)
                    return Result.Fail<BookingVoucherData>(bookingError);

                var (_, isAccommodationFailure, accommodationDetails, accommodationError) = await _accommodationService.Get(booking.DataProvider, bookingDetails.AccommodationId, languageCode);
                if (isAccommodationFailure)
                    return Result.Fail<BookingVoucherData>(accommodationError.Detail);

                return Result.Ok(new BookingVoucherData
                (
                    booking.Id,
                    GetAccommodationInfo(in accommodationDetails),
                    bookingDetails.CheckInDate,
                    bookingDetails.CheckOutDate,
                    serviceDetails.DeadlineDetails,
                    booking.MainPassengerName,
                    booking.ReferenceCode,
                    bookingDetails.RoomDetails.Select(i => i.RoomDetails).ToList()
                ));
            }
        }


        private BookingVoucherData.AccommodationInfo GetAccommodationInfo(in AccommodationDetails details)
        {
            var location = new SlimLocationInfo(details.Location.Address, details.Location.Country, details.Location.Locality, details.Location.LocalityZone, details.Location.Coordinates);
            return new BookingVoucherData.AccommodationInfo(details.Name, in location, details.Contacts);
        }


        public Task<Result<BookingInvoiceData>> GenerateInvoice(int bookingId, string languageCode)
        {
            return GetBookingData(bookingId)
                .OnSuccess(CreateInvoiceData);


            Result<BookingInvoiceData> CreateInvoiceData(
                (AccommodationBookingInfo bookingInfo, BookingAvailabilityInfo serviceDetails, AccommodationBookingDetails bookingDetails) bookingData)
            {
                var serviceDetails = bookingData.serviceDetails;
                var bookingDetails = bookingData.bookingDetails;

                return Result.Ok(new BookingInvoiceData
                {
                    CheckInDate = bookingDetails.CheckInDate.ToString("d"),
                    CheckOutDate = bookingDetails.CheckOutDate.ToString("d"),
                    RoomDetails = bookingDetails.RoomDetails,
                    CurrencyCode = Currencies.ToCurrencyCode(serviceDetails.Agreement.Price.Currency),
                    PriceTotal = serviceDetails.Agreement.Price.NetTotal.ToString(CultureInfo.InvariantCulture),
                    AccommodationName = serviceDetails.AccommodationName
                });
            }
        }


        private async Task<Result<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>> GetBookingData(int bookingId)
        {
            var (_, isFailure, bookingInfo, error) = await _bookingManager.GetCustomerBookingInfo(bookingId);

            if (isFailure)
                return Result.Fail<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>(error);

            return Result.Ok((bookingInfo, bookingInfo.ServiceDetails, bookingInfo.BookingDetails));
        }


        private readonly IAccommodationService _accommodationService;
        private readonly IBookingManager _bookingManager;
    }
}