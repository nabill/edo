using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorBookingManagementService : IAdministratorBookingManagementService
    {
        public AdministratorBookingManagementService(IBookingRecordManager recordManager,
            IBookingManagementService managementService,
            IDateTimeProvider dateTimeProvider,
            IBookingRecordsUpdater recordsUpdater)
        {
            _recordManager = recordManager;
            _managementService = managementService;
            _dateTimeProvider = dateTimeProvider;
            _recordsUpdater = recordsUpdater;
        }
        
        public Task<Result> Discard(int bookingId, Administrator administrator)
        {
            return GetBooking(bookingId)
                .Bind(ProcessDiscard);

            
            Task<Result> ProcessDiscard(Booking booking) 
                => _recordsUpdater.ChangeStatus(booking, BookingStatuses.Discarded, _dateTimeProvider.UtcNow(), administrator.ToUserInfo(), BookingChangeReasons.ChangedByAdministrator);
        }


        public Task<Result> RefreshStatus(int bookingId, Administrator admin)
        {
            return GetBooking(bookingId)
                .Bind(ProcessRefresh);
            
            
            Task<Result> ProcessRefresh(Booking booking) 
                => _managementService.RefreshStatus(booking, admin.ToUserInfo());
        }


        public async Task<Result> Cancel(int bookingId, Administrator admin)
        {
            return await GetBooking(bookingId)
                .Bind(Cancel);
                
            
            Task<Result> Cancel(Booking booking) 
                => _managementService.Cancel(booking, admin.ToUserInfo());
        }


        public async Task<Result> CancelManually(int bookingId, DateTime cancellationDate, string reason, Administrator admin)
        {
            // TODO: AA-26 Store cancellation reason
            return await GetBooking(bookingId)
                .Bind(CancelManually);


            Task<Result> CancelManually(Booking booking)
                => _recordsUpdater.ChangeStatus(booking, BookingStatuses.Cancelled, cancellationDate, admin.ToUserInfo(), BookingChangeReasons.ChangedByAdministrator);
        }
        
        
        public async Task<Result> RejectManually(int bookingId, string reason, Administrator admin)
        {
            // TODO: AA-26 Store rejection reason
            return await GetBooking(bookingId)
                .Bind(RejectManually);


            Task<Result> RejectManually(Booking booking)
                => _recordsUpdater.ChangeStatus(booking, BookingStatuses.Rejected, _dateTimeProvider.UtcNow(), admin.ToUserInfo(), BookingChangeReasons.ChangedByAdministrator);
        }


        private Task<Result<Booking>> GetBooking(int id) 
            => _recordManager.Get(id);
        
        
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingManagementService _managementService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRecordsUpdater _recordsUpdater;
    }
}