using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Payments.Offline;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingOfflinePaymentService : IBookingOfflinePaymentService
    {
        public BookingOfflinePaymentService(IBookingRecordManager recordManager,
            IAdminAgencyManagementService adminAgencyManagementService,
            IOfflinePaymentAuditService auditService,
            EdoContext context)
        {
            _recordManager = recordManager;
            _adminAgencyManagementService = adminAgencyManagementService;
            _auditService = auditService;
            _context = context;
        }
        
        
        public async Task<Result> CompleteOffline(int bookingId, Administrator administratorContext)
        {
            return await GetBooking()
                .Check(CheckBookingPaymentStatus)
                .Check(CheckAgencyKontractKind)
                .Tap(Complete)
                .Tap(WriteAuditLog);


            async Task<Result<Booking>> GetBooking()
            {
                var (_, isFailure, booking, _) = await _recordManager.Get(bookingId);
                return isFailure
                    ? Result.Failure<Booking>($"Could not find booking with id {bookingId}")
                    : Result.Success(booking);
            }


            Result CheckBookingPaymentStatus(Booking booking)
                => booking.PaymentStatus == BookingPaymentStatuses.NotPaid
                    ? Result.Success()
                    : Result.Failure($"Could not complete booking. Invalid payment status: {booking.PaymentStatus}");


            async Task<Result> CheckAgencyKontractKind(Booking booking)
            {
                var (_, isFailure, agency, error) = await _adminAgencyManagementService.Get(booking.AgencyId);
                if (isFailure)
                    return Result.Failure(error);

                if (agency.ContractKind != Data.Agents.ContractKind.OfflineOrCreditCardPayments)                        
                    return Result.Failure($"Could not complete booking. Invalid agency contract kind: {agency.ContractKind}");

                return Result.Success();
            }


            Task Complete(Booking booking)
            {
                booking.PaymentType = PaymentTypes.Offline;
                return ChangeBookingPaymentStatusToCaptured(booking);
            }


            Task WriteAuditLog(Booking booking) 
                => _auditService.Write(administratorContext.ToApiCaller(), booking.ReferenceCode);
            

            Task ChangeBookingPaymentStatusToCaptured(Booking booking)
            {
                booking.PaymentStatus = BookingPaymentStatuses.Captured;
                _context.Bookings.Update(booking);
                return _context.SaveChangesAsync();
            }
        }
        
        
        private readonly IBookingRecordManager _recordManager;
        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
        private readonly IOfflinePaymentAuditService _auditService;
        private readonly EdoContext _context;
    }
}