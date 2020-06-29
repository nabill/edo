using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingDocumentsService : IBookingDocumentsService
    {
        public BookingDocumentsService(IOptions<BankDetails> bankDetails, 
            IBookingRecordsManager bookingRecordsManager, 
            IAccommodationService accommodationService,
            ICounterpartyService counterpartyService,
            IInvoiceService invoiceService,
            IReceiptService receiptService)
        {
            _bankDetails = bankDetails.Value;
            _bookingRecordsManager = bookingRecordsManager;
            _accommodationService = accommodationService;
            _counterpartyService = counterpartyService;
            _invoiceService = invoiceService;
            _receiptService = receiptService;
        }


        public async Task<Result<BookingVoucherData>> GenerateVoucher(int bookingId, AgentContext agent, string languageCode)
        {
            var (_, isBookingFailure, booking, bookingError) = await _bookingRecordsManager.Get(bookingId, agent.AgentId);
            if (isBookingFailure)
                return Result.Failure<BookingVoucherData>(bookingError);

            var (_, isAccommodationFailure, accommodationDetails, accommodationError) = await _accommodationService.Get(booking.DataProvider, 
                booking.AccommodationId, languageCode);
                
            if(isAccommodationFailure)
                return Result.Failure<BookingVoucherData>(accommodationError.Detail);

            
            return Result.Ok(new BookingVoucherData
            (
                $"{agent.LastName} {agent.LastName}",
                booking.Id,
                GetAccommodationInfo(in accommodationDetails),
                (booking.CheckOutDate - booking.CheckInDate).Days,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.DeadlineDate,
                booking.MainPassengerName,
                booking.ReferenceCode,
                booking.Rooms.Select(r=> new BookingVoucherData.RoomInfo(r.Type,
                    r.BoardBasis,
                    r.MealPlan,
                    r.DeadlineDate,
                    r.ContractDescription,
                    r.Passengers,
                    r.Remarks))
                    .ToList()
            )); 
        }
        
        private static BookingVoucherData.AccommodationInfo GetAccommodationInfo(in AccommodationDetails details)
        {
            var location = new SlimLocationInfo(details.Location.Address, details.Location.Country, details.Location.Locality, details.Location.LocalityZone, details.Location.Coordinates);
            return new BookingVoucherData.AccommodationInfo(details.Name, in location, details.Contacts);
        }


        public async Task<Result<(DocumentRegistrationInfo RegistrationInfo, BookingInvoiceData Data)>> GetActualInvoice(int bookingId, AgentContext agent)
        {
            var (_, isFailure, booking, _) = await _bookingRecordsManager.Get(bookingId, agent.AgentId);
            if (isFailure)
                return Result.Failure<(DocumentRegistrationInfo Metadata, BookingInvoiceData Data)>("Could not find booking");
            
            return await GetActualInvoice(booking);
        }


        public async Task<Result> GenerateInvoice(string referenceCode)
        {
            var (_, isBookingFailure, booking, bookingError) = await _bookingRecordsManager.Get(referenceCode);
            if (isBookingFailure)
                return Result.Failure(bookingError);

            var (_, isCounterpartyFailure, counterparty, counterpartyError) = await _counterpartyService.Get(booking.CounterpartyId);
            if (isCounterpartyFailure)
                return Result.Failure(counterpartyError);

            var invoiceData = new BookingInvoiceData(
                GetBuyerInfo(in counterparty),
                GetSellerDetails(booking, _bankDetails),
                booking.ReferenceCode,
                GetRows(booking.AccommodationName, booking.Rooms),
                new MoneyAmount(booking.TotalPrice, booking.Currency),
                booking.DeadlineDate ?? booking.CheckInDate
            );

            await _invoiceService.Register(ServiceTypes.HTL, ServiceSource.Internal, booking.ReferenceCode, invoiceData);
            return Result.Ok();
            
            static List<BookingInvoiceData.InvoiceItemInfo> GetRows(string accommodationName, List<BookedRoom> bookingRooms)
            {
                return bookingRooms
                    .Select((room, counter) =>
                    {
                        return new BookingInvoiceData.InvoiceItemInfo(counter + 1,
                            accommodationName,
                            room.ContractDescription,
                            room.Price,
                            room.Price
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

            static BookingInvoiceData.BuyerInfo GetBuyerInfo(in CounterpartyInfo counterparty) => new BookingInvoiceData.BuyerInfo(counterparty.Name, 
                counterparty.Address,
                counterparty.Phone,
                counterparty.BillingEmail);
        }
        

        public async Task<Result<(DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data)>> GenerateReceipt(int bookingId)
        {
            var (_, isBookingFailure, booking, bookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isBookingFailure)
                return Result.Failure<(DocumentRegistrationInfo, PaymentReceipt)>(bookingError);
            
            var (_, isInvoiceFailure, invoiceInfo, invoiceError) = await GetActualInvoice(booking);
            if(isInvoiceFailure)
                return Result.Failure<(DocumentRegistrationInfo, PaymentReceipt)>(invoiceError);
            
            var receiptData = new PaymentReceipt(booking.TotalPrice, 
                booking.Currency,
                booking.PaymentMethod,
                booking.ReferenceCode);

            var (_, isRegistrationFailure, regInfo, registrationError) = await _receiptService.Register(invoiceInfo.RegistrationInfo.Number, receiptData);
            if(isRegistrationFailure)
                return Result.Failure<(DocumentRegistrationInfo, PaymentReceipt)>(registrationError);
                
            return Result.Ok((regInfo, receiptData));
        }
        
        
        private async Task<Result<(DocumentRegistrationInfo RegistrationInfo, BookingInvoiceData Data)>> GetActualInvoice(Booking booking)
        {
            var lastInvoice = (await _invoiceService.Get<BookingInvoiceData>(ServiceTypes.HTL, ServiceSource.Internal, booking.ReferenceCode))
                .OrderByDescending(i => i.Metadata.Date)
                .LastOrDefault();

            return lastInvoice.Equals(default)
                ? Result.Failure<(DocumentRegistrationInfo Metadata, BookingInvoiceData Data)>("Could not find invoice")
                : Result.Ok(lastInvoice);
        }


        private readonly BankDetails _bankDetails;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IAccommodationService _accommodationService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly IInvoiceService _invoiceService;
        private readonly IReceiptService _receiptService;
    }
}