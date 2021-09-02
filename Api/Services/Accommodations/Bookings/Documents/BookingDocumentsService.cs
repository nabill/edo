using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Booking = HappyTravel.Edo.Data.Bookings.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents
{
    public class BookingDocumentsService : IBookingDocumentsService
    {
        public BookingDocumentsService(EdoContext context,
            IOptions<BankDetails> bankDetails, 
            IAccommodationService accommodationService,
            ICounterpartyService counterpartyService,
            IInvoiceService invoiceService,
            IReceiptService receiptService,
            IImageFileService imageFileService)
        {
            _context = context;
            _bankDetails = bankDetails.Value;
            _accommodationService = accommodationService;
            _counterpartyService = counterpartyService;
            _invoiceService = invoiceService;
            _receiptService = receiptService;
            _imageFileService = imageFileService;
        }


        public async Task<Result<BookingVoucherData>> GenerateVoucher(Booking booking, string languageCode)
        {
            var (_, isAccommodationFailure, accommodationDetails, accommodationError) = await _accommodationService.Get(booking.HtId, languageCode);
                
            if (isAccommodationFailure)
                return Result.Failure<BookingVoucherData>(accommodationError.Detail);

            var bannerMaybe = await _imageFileService.GetBanner(booking.AgencyId);
            var logoMaybe = await _imageFileService.GetLogo(booking.AgencyId);
            var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
            if (agent == default)
                return Result.Failure<BookingVoucherData>("Could not find agent");

            if (!AvailableForVoucherBookingStatuses.Contains(booking.Status))
                return Result.Failure<BookingVoucherData>($"Voucher is not allowed for booking status '{EnumFormatters.FromDescription(booking.Status)}'");

            if (!AvailableForVoucherPaymentStatuses.Contains(booking.PaymentStatus))
                return Result.Failure<BookingVoucherData>($"Voucher is not allowed for payment status '{EnumFormatters.FromDescription(booking.PaymentStatus)}'");

            return new BookingVoucherData
            (
                $"{agent.FirstName} {agent.LastName}",
                booking.Id,
                GetAccommodationInfo(accommodationDetails),
                (booking.CheckOutDate - booking.CheckInDate).Days,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.DeadlineDate,
                booking.MainPassengerName,
                booking.ReferenceCode,
                booking.SupplierReferenceCode,
                bannerMaybe.HasValue ? bannerMaybe.Value.Url : null,
                logoMaybe.HasValue ? logoMaybe.Value.Url : null,
                booking.Rooms.Select(r=> new BookingVoucherData.RoomInfo(r.ContractDescription,
                    r.BoardBasis,
                    r.MealPlan,
                    r.DeadlineDate,
                    r.ContractDescription,
                    r.Passengers,
                    r.Remarks,
                    r.SupplierRoomReferenceCode))
                    .ToList()
            ); 
        }
        
        private static BookingVoucherData.AccommodationInfo GetAccommodationInfo(in Accommodation details)
        {
            var location = new SlimLocationInfo(details.Location.Address, details.Location.Country, details.Location.CountryCode, details.Location.Locality, details.Location.LocalityZone, details.Location.Coordinates);
            return new BookingVoucherData.AccommodationInfo(details.Name, in location, 
                details.Contacts, details.Schedule.CheckInTime, details.Schedule.CheckOutTime);
        }


        public async Task<Result> GenerateInvoice(Data.Bookings.Booking booking)
        {
            var rootAgency = await _counterpartyService.GetRootAgency(booking.CounterpartyId);

            var invoiceData = new BookingInvoiceData(
                GetBuyerInfo(rootAgency),
                GetSellerDetails(booking, _bankDetails),
                booking.ReferenceCode,
                booking.SupplierReferenceCode,
                GetRows(booking.AccommodationName, booking.Rooms),
                new MoneyAmount(booking.TotalPrice, booking.Currency),
                booking.DeadlineDate ?? booking.CheckInDate,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.PaymentStatus,
                booking.DeadlineDate
            );

            await _invoiceService.Register(ServiceTypes.HTL, ServiceSource.Internal, booking.ReferenceCode, invoiceData);
            return Result.Success();


            static List<BookingInvoiceData.InvoiceItemInfo> GetRows(string accommodationName, List<BookedRoom> bookingRooms)
            {
                return bookingRooms
                    .Select((room, counter) =>
                        new BookingInvoiceData.InvoiceItemInfo(counter + 1,
                            accommodationName,
                            room.ContractDescription,
                            room.Price,
                            room.Price,
                            room.Type,
                            room.DeadlineDate,
                            room.Passengers.Where(p => p.IsLeader).Select(p => p.FirstName).SingleOrDefault(),
                            room.Passengers.Where(p => p.IsLeader).Select(p => p.LastName).SingleOrDefault()
                        ))
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

            static BookingInvoiceData.BuyerInfo GetBuyerInfo(Agency agency) => new BookingInvoiceData.BuyerInfo(agency.Name, 
                agency.Address,
                agency.Phone,
                agency.BillingEmail);
        }


        public async Task<Result<(DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data)>> GenerateReceipt(Data.Bookings.Booking booking)
        {
            var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);

          var (_, isInvoiceFailure, invoiceInfo, invoiceError) = await GetActualInvoice(booking);
            if (isInvoiceFailure)
                return Result.Failure<(DocumentRegistrationInfo, PaymentReceipt)>(invoiceError);
            
            var receiptData = new PaymentReceipt(booking.TotalPrice, 
                booking.Currency,
                booking.PaymentType,
                booking.ReferenceCode,
                invoiceInfo.RegistrationInfo,
                booking.AccommodationName,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.DeadlineDate,
                booking.Rooms.Select(r => new PaymentReceipt.ReceiptItemInfo(r.DeadlineDate, r.Type)).ToList(),
                new PaymentReceipt.BuyerInfo(
                    invoiceInfo.Data.BuyerDetails.Name,
                    invoiceInfo.Data.BuyerDetails.Address,
                    invoiceInfo.Data.BuyerDetails.ContactPhone,
                    invoiceInfo.Data.BuyerDetails.Email),
                $"{agent.FirstName} {agent.LastName}");

            var (_, isRegistrationFailure, regInfo, registrationError) = await _receiptService.Register(invoiceInfo.RegistrationInfo.Number, receiptData);
            if (isRegistrationFailure)
                return Result.Failure<(DocumentRegistrationInfo, PaymentReceipt)>(registrationError);
                
            return Result.Success((regInfo, receiptData));
        }
        
        
        public async Task<Result<(DocumentRegistrationInfo RegistrationInfo, BookingInvoiceData Data)>> GetActualInvoice(Booking booking)
        {
            var lastInvoice = (await _invoiceService.Get<BookingInvoiceData>(ServiceTypes.HTL, ServiceSource.Internal, booking.ReferenceCode))
                .OrderBy(i => i.Metadata.Date)
                .LastOrDefault();

            return lastInvoice.Equals(default)
                ? Result.Failure<(DocumentRegistrationInfo Metadata, BookingInvoiceData Data)>("Could not find invoice")
                : Result.Success(lastInvoice);
        }


        private static readonly HashSet<BookingStatuses> AvailableForVoucherBookingStatuses = new()
        {
            BookingStatuses.Confirmed
        };

        private static readonly HashSet<BookingPaymentStatuses> AvailableForVoucherPaymentStatuses = new()
        {
            BookingPaymentStatuses.Authorized,
            BookingPaymentStatuses.Captured
        };

        private readonly EdoContext _context;
        private readonly BankDetails _bankDetails;
        private readonly IAccommodationService _accommodationService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly IInvoiceService _invoiceService;
        private readonly IReceiptService _receiptService;
        private readonly IImageFileService _imageFileService;
    }
}