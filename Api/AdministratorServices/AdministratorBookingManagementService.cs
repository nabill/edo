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
        public AdministratorBookingManagementService(IBookingRecordsManager recordsManager,
            IBookingManagementService managementService)
        {
            _recordsManager = recordsManager;
            _managementService = managementService;
        }
        
        public async Task<Result> Discard(int bookingId, Administrator administrator)
        {
            return await GetBooking(bookingId)
                .Tap(ProcessDiscard);

            
            Task ProcessDiscard(Booking booking) 
                => _managementService.Discard(booking, administrator.ToUserInfo());
        }
        
        
        public async Task<Result> Cancel(int bookingId, Administrator admin, bool requireSupplierConfirmation)
        {
            return await GetBooking(bookingId)
                .Bind(ProcessCancel);
                
            
            Task<Result> ProcessCancel(Booking booking) 
                => _managementService.Cancel(booking, admin.ToUserInfo());
        }


        private Task<Result<Booking>> GetBooking(int id) 
            => _recordsManager.Get(id);
        
        
        private readonly IBookingRecordsManager _recordsManager;
        private readonly IBookingManagementService _managementService;
    }
}