using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorBookingManagementService : IAdministratorBookingManagementService
    {
        public AdministratorBookingManagementService(IBookingRecordManager recordManager,
            IBookingManagementService managementService)
        {
            _recordManager = recordManager;
            _managementService = managementService;
        }
        
        public Task<Result> Discard(int bookingId, Administrator administrator)
        {
            return GetBooking(bookingId)
                .Bind(ProcessDiscard);

            
            Task<Result> ProcessDiscard(Booking booking) 
                => _managementService.Discard(booking, administrator.ToUserInfo());
        }


        public Task<Result> RefreshStatus(int bookingId, Administrator admin)
        {
            return GetBooking(bookingId)
                .Bind(ProcessRefresh);
            
            
            Task<Result> ProcessRefresh(Booking booking) 
                => _managementService.RefreshStatus(booking, admin.ToUserInfo());
        }


        public async Task<Result> Cancel(int bookingId, Administrator admin, bool requireSupplierConfirmation)
        {
            return await GetBooking(bookingId)
                .Bind(ProcessCancel);
                
            
            Task<Result> ProcessCancel(Booking booking) 
                => _managementService.Cancel(booking, admin.ToUserInfo());
        }


        private Task<Result<Booking>> GetBooking(int id) 
            => _recordManager.Get(id);
        
        
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingManagementService _managementService;
    }
}