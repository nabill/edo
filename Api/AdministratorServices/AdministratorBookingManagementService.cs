using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorBookingManagementService : IAdministratorBookingManagementService
    {
        public AdministratorBookingManagementService(IBookingRecordManager recordManager,
            IBookingManagementService managementService,
            IDateTimeProvider dateTimeProvider,
            IBookingRecordsUpdater recordsUpdater,
            EdoContext context)
        {
            _recordManager = recordManager;
            _managementService = managementService;
            _dateTimeProvider = dateTimeProvider;
            _recordsUpdater = recordsUpdater;
            _context = context;
        }
        
        public Task<Result> Discard(int bookingId, Administrator admin)
        {
            return GetBooking(bookingId)
                .Bind(ProcessDiscard)
                .Tap(WriteLog);

            
            Task<Result> ProcessDiscard(Booking booking) 
                => _recordsUpdater.ChangeStatus(booking, BookingStatuses.Discarded, _dateTimeProvider.UtcNow(), admin.ToUserInfo());


            Task WriteLog() 
                => WriteAuditLog(bookingId, admin, BookingManagementOperationTypes.Discard);
        }


        public Task<Result> RefreshStatus(int bookingId, Administrator admin)
        {
            return GetBooking(bookingId)
                .Bind(ProcessRefresh)
                .Tap(WriteLog);
            
            
            Task<Result> ProcessRefresh(Booking booking) 
                => _managementService.RefreshStatus(booking, admin.ToUserInfo());
            
            
            Task WriteLog() 
                => WriteAuditLog(bookingId, admin, BookingManagementOperationTypes.RefreshStatus);
        }


        public async Task<Result> Cancel(int bookingId, Administrator admin)
        {
            return await GetBooking(bookingId)
                .Bind(Cancel)
                .Tap(WriteLog);
                
            
            Task<Result> Cancel(Booking booking) 
                => _managementService.Cancel(booking, admin.ToUserInfo());
            
            
            Task WriteLog() 
                => WriteAuditLog(bookingId, admin, BookingManagementOperationTypes.Cancel);
        }


        public async Task<Result> CancelManually(int bookingId, DateTime cancellationDate, string reason, Administrator admin)
        {
            return await GetBooking(bookingId)
                .Bind(CancelManually)
                .Tap(WriteLog);


            Task<Result> CancelManually(Booking booking)
                => _recordsUpdater.ChangeStatus(booking, BookingStatuses.Cancelled, cancellationDate, admin.ToUserInfo());
            
            
            Task WriteLog() 
                => WriteAuditLog(bookingId, admin, BookingManagementOperationTypes.CancelManually, reason, cancellationDate);
        }
        
        
        public async Task<Result> RejectManually(int bookingId, string reason, Administrator admin)
        {
            return await GetBooking(bookingId)
                .Bind(RejectManually)
                .Tap(WriteLog);


            Task<Result> RejectManually(Booking booking)
                => _recordsUpdater.ChangeStatus(booking, BookingStatuses.Rejected, _dateTimeProvider.UtcNow(), admin.ToUserInfo());
            
            
            Task WriteLog() 
                => WriteAuditLog(bookingId, admin, BookingManagementOperationTypes.RejectManually, reason);
        }


        public async Task<Result> ConfirmManually(int bookingId, DateTime confirmationDate, string reason, Administrator admin)
        {
            return await GetBooking(bookingId)
                .Bind(ConfirmManually)
                .Tap(WriteLog);


            Task<Result> ConfirmManually(Booking booking)
                => _recordsUpdater.ChangeStatus(booking, BookingStatuses.Confirmed, confirmationDate, admin.ToUserInfo());
            
            
            Task WriteLog() 
                => WriteAuditLog(bookingId, admin, BookingManagementOperationTypes.ConfirmManually, reason, confirmationDate);
        }


        private Task<Result<Booking>> GetBooking(int id) 
            => _recordManager.Get(id);


        private async Task WriteAuditLog(int bookingId, Administrator admin, BookingManagementOperationTypes type, string reason = default, DateTime? date = null)
        {
            var logEntry = new BookingManagementAuditLogEntry
            {
                BookingId = bookingId,
                AdministratorId = admin.Id,
                Reason = reason,
                Date = date,
                Created = _dateTimeProvider.UtcNow(),
                OperationType = type
            };
            _context.BookingManagementAuditLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }


        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingManagementService _managementService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRecordsUpdater _recordsUpdater;
        private readonly EdoContext _context;
    }
}