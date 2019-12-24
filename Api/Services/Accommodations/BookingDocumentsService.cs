using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Mailing;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class BookingDocumentsService : IBookingDocumentsService
    {
        public BookingDocumentsService(IAccommodationBookingManager bookingManager)
        {
            _bookingManager = bookingManager;
        }

        public Task<Result<BookingVoucherData>> GenerateVoucher(int bookingId)
        {
            return GetBookingData(bookingId)
                .OnSuccess(CreateVoucherData);
            
            
            Result<BookingVoucherData> CreateVoucherData(
                (AccommodationBookingInfo bookingInfo, BookingAvailabilityInfo serviceDetails, AccommodationBookingDetails bookingDetails) bookingData)
            {
                var serviceDetails = bookingData.serviceDetails;
                var bookingDetails = bookingData.bookingDetails;
                var bookingInfo = bookingData.bookingInfo;

                return Result.Ok(new BookingVoucherData
                {
                    BookingId = bookingInfo.BookingId.ToString(),
                    CheckInDate = bookingDetails.CheckInDate.ToString("d"),
                    CheckOutDate = bookingDetails.CheckOutDate.ToString("d"),
                    ReferenceCode = bookingDetails.ReferenceCode,
                    RoomDetails = bookingDetails.RoomDetails.Select(i => i.RoomDetails).ToList(),

                    AccomodationName = serviceDetails.AccommodationName,
                    LocationName = serviceDetails.CityCode,
                    CountryName = serviceDetails.CountryName,
                    BoardBasis = serviceDetails.Agreement.BoardBasis,
                    BoardBasisCode = serviceDetails.Agreement.BoardBasisCode,
                    ContractType = serviceDetails.Agreement.ContractType,
                    MealPlan = serviceDetails.Agreement.MealPlan,
                    MealPlanCode = serviceDetails.Agreement.MealPlanCode,
                    TariffCode = serviceDetails.Agreement.TariffCode,
                });
            }
        }


        public Task<Result<BookingInvoiceData>> GenerateInvoice(int bookingId)
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
                    RoomDetails = bookingDetails.RoomDetails.Select(i => i.RoomDetails).ToList(),
                    CurrencyCode = serviceDetails.Agreement.Price.CurrencyCode,
                    PriceTotal = serviceDetails.Agreement.Price.NetTotal.ToString(CultureInfo.InvariantCulture),
                    PriceGross = serviceDetails.Agreement.Price.Gross.ToString(CultureInfo.InvariantCulture),
                    TariffCode = serviceDetails.Agreement.TariffCode,
                    AccomodationName = serviceDetails.AccommodationName,
                    LocationName = serviceDetails.CityCode,
                    CountryName = serviceDetails.CountryName
                });
            }
        }
        
        private async Task<Result<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>> GetBookingData(int bookingId)
        {
            var (_, isFailure, bookingInfo, error) = await _bookingManager.Get(bookingId);

            if (isFailure)
                return Result.Fail<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>(error);

            return Result.Ok((bookingInfo, bookingInfo.ServiceDetails, bookingInfo.BookingDetails));
        }
        
        private readonly IAccommodationBookingManager _bookingManager;
    }
}