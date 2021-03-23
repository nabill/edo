using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    /// <summary>
    /// Common booking management service to be used by agents, administrators, service accounts
    /// </summary>
    public interface IBookingManagementService
    {
        Task<Result> Cancel(Booking booking, UserInfo user);
        
        Task<Result> RefreshStatus(Booking booking, UserInfo user, BookingChangeReason changeReason);
    }
}