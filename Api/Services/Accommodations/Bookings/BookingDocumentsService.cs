using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.MailSender.Formatters;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingDocumentsService : IBookingDocumentsService
    {
        public BookingDocumentsService(IOptions<BankDetails> bankDetails, 
            IBookingRecordsManager bookingRecordsManager, 
            IAccommodationService accommodationService,
            ICounterpartyService counterpartyService)
        {
            _bankDetails = bankDetails.Value;
            _bookingRecordsManager = bookingRecordsManager;
            _accommodationService = accommodationService;
            _counterpartyService = counterpartyService;
        }


        public async Task<Result<BookingVoucherData>> GenerateVoucher(int bookingId, string languageCode)
        {
            var (_, isBookingFailure, booking, bookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isBookingFailure)
                return Result.Fail<BookingVoucherData>(bookingError);

            var (_, isAccommodationFailure, accommodationDetails, accommodationError) = await _accommodationService.Get(booking.DataProvider, 
                booking.AccommodationId, languageCode);
            
            if (isAccommodationFailure)
                return Result.Fail<BookingVoucherData>(accommodationError.Detail);

            return Result.Ok(new BookingVoucherData
            (
                booking.Id,
                GetAccommodationInfo(in accommodationDetails),
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.DeadlineDate,
                booking.MainPassengerName,
                booking.ReferenceCode,
                booking.Rooms,
                accommodationDetails.Name
            )); 
        }
        
        private static BookingVoucherData.AccommodationInfo GetAccommodationInfo(in AccommodationDetails details)
        {
            var location = new SlimLocationInfo(details.Location.Address, details.Location.Country, details.Location.Locality, details.Location.LocalityZone, details.Location.Coordinates);
            return new BookingVoucherData.AccommodationInfo(details.Name, in location, details.Contacts);
        }


        public async Task<Result<BookingInvoiceData>> GenerateInvoice(int bookingId, string languageCode)
        {
            var (_, isBookingFailure, booking, bookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isBookingFailure)
                return Result.Fail<BookingInvoiceData>(bookingError);

            var (_, isCounterpartyFailure, counterparty, counterpartyError) = await _counterpartyService.Get(booking.CounterpartyId);
            if (isCounterpartyFailure)
                return Result.Fail<BookingInvoiceData>(counterpartyError);

            return Result.Ok(new BookingInvoiceData(
                booking.Id, 
                GetBuyerInfo(in counterparty),
                GetSellerDetails(booking, _bankDetails),
                booking.ReferenceCode, 
                GetRows(booking.AccommodationName, booking.Rooms), 
                booking.Created.ToString("dd MMMM yyyy"),
                (booking.DeadlineDate ?? booking.CheckInDate).ToString("dd MMMM yyyy")
                ));
            
            static List<BookingInvoiceData.InvoiceItem> GetRows(string accommodationName, List<BookedRoom> bookingRooms)
            {
                return bookingRooms
                    .Select((room, counter) =>
                    {
                        var (amount, currency) = room.Price;
                        var price = EmailContentFormatter.FromAmount(amount, currency);
                        return new BookingInvoiceData.InvoiceItem(counter + 1,
                            accommodationName,
                            room.ContractDescription,
                            price,
                            price
                        );
                    })
                    .ToList();
            }


            static BookingInvoiceData.SellerInfo GetSellerDetails(Booking booking, BankDetails bankDetails)
            {
                if (!bankDetails.AccountDetails.TryGetValue(booking.Currency, out var accountData))
                    accountData = bankDetails.AccountDetails[Currencies.USD];

                var sellerDetails = new BookingInvoiceData.SellerInfo(bankDetails.CompanyName,
                    bankDetails.BankName, 
                    bankDetails.BankAddress,
                    accountData.AccountNumber,
                    accountData.Iban, 
                    bankDetails.RoutingCode,
                    bankDetails.SwiftCode);
                
                return sellerDetails;
            }

            // TODO: add a contact number and a billing email after company table refactoring
            static BookingInvoiceData.BuyerInfo GetBuyerInfo(in CounterpartyInfo counterparty) => new BookingInvoiceData.BuyerInfo(counterparty.Name, 
                counterparty.Address,
                counterparty.Phone,
                "billingEmail@mail.com");
        }

        private readonly BankDetails _bankDetails;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IAccommodationService _accommodationService;
        private readonly ICounterpartyService _counterpartyService;
    }
}