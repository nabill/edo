using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingDocumentsService : IBookingDocumentsService
    {
        public BookingDocumentsService(IOptions<BankDetails> bankDetails, IBookingManager bookingManager, IAccommodationService accommodationService, ICompanyService companyService)
        {
            _accommodationService = accommodationService;
            _bookingManager = bookingManager;
            _companyService = companyService;

            _bankDetails = bankDetails.Value;
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


        public Task<Result<BookingInvoiceData>> GenerateInvoice(int bookingId, string languageCode)
        {
            return GetBookingData(bookingId)
                .OnSuccess(CreateInvoiceData);


            async Task<Result<BookingInvoiceData>> CreateInvoiceData(
                (AccommodationBookingInfo bookingInfo, BookingAvailabilityInfo serviceDetails, AccommodationBookingDetails bookingDetails) bookingData)
            {
                var serviceDetails = bookingData.serviceDetails;
                var bookingDetails = bookingData.bookingDetails;

                var (_, isBookingFailure, booking, bookingError) = await _bookingManager.Get(bookingId);
                if (isBookingFailure)
                    return Result.Fail<BookingInvoiceData>(bookingError);

                var (_, isAccommodationFailure, accommodationDetails, accommodationError) =
                    await _accommodationService.Get(booking.DataProvider, bookingDetails.AccommodationId, languageCode);
                if (isAccommodationFailure)
                    return Result.Fail<BookingInvoiceData>(accommodationError.Detail);

                var (_, isCompanyFailure, company, companyError) = await _companyService.Get(booking.CompanyId);
                if (isCompanyFailure)
                    return Result.Fail<BookingInvoiceData>(companyError);

                if (!_bankDetails.AccountDetails.TryGetValue(serviceDetails.Agreement.Price.Currency, out var accountData))
                    _bankDetails.AccountDetails.TryGetValue(Currencies.USD, out accountData);
                
                var sellerDetails = new BookingInvoiceData.SellerInfo(_bankDetails.CompanyName, _bankDetails.BankName, _bankDetails.BankAddress,
                    accountData.AccountNumber, accountData.Iban, _bankDetails.RoutingCode, _bankDetails.SwiftCode);

                //TODO: add a contract number and a billing email after company table refactoring
                var buyerDetails = new BookingInvoiceData.BuyerInfo(company.Name, company.Address, "contractNumber", "billingEmail");

                var roomDetails = new List<BookingRoomDetails>();


                /*CheckInDate = bookingDetails.CheckInDate.ToString("d"),
                CheckOutDate = bookingDetails.CheckOutDate.ToString("d"),
                RoomDetails = bookingDetails.RoomDetails,
                CurrencyCode = Currencies.ToCurrencyCode(serviceDetails.Agreement.Price.Currency),
                PriceTotal = serviceDetails.Agreement.Price.NetTotal.ToString(CultureInfo.InvariantCulture),
                AccommodationName = serviceDetails.AccommodationName*/
                return Result.Ok(new BookingInvoiceData(booking.Id, in buyerDetails, in sellerDetails, booking.ReferenceCode, roomDetails, booking.Created,
                    bookingDetails.Deadline));
            }
        }


        private static BookingVoucherData.AccommodationInfo GetAccommodationInfo(in AccommodationDetails details)
        {
            var location = new SlimLocationInfo(details.Location.Address, details.Location.Country, details.Location.Locality, details.Location.LocalityZone, details.Location.Coordinates);
            return new BookingVoucherData.AccommodationInfo(details.Name, in location, details.Contacts);
        }


        private async Task<Result<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>> GetBookingData(int bookingId)
        {
            var (_, isFailure, bookingInfo, error) = await _bookingManager.GetCustomerBookingInfo(bookingId);
            if (isFailure)
                return Result.Fail<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>(error);

            return Result.Ok((bookingInfo, bookingInfo.ServiceDetails, bookingInfo.BookingDetails));
        }


        private readonly IAccommodationService _accommodationService;
        private readonly BankDetails _bankDetails;
        private readonly IBookingManager _bookingManager;
        private readonly ICompanyService _companyService;
    }
}