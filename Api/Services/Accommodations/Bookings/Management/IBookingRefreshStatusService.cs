using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IBookingStatusRefreshService
    {
        Task<Result> RefreshStatus(Booking booking, UserInfo userInfo, List<BookingStatusRefreshState> states = null);

        Task<Result<BatchOperationResult>> RefreshStatuses(List<int> bookingIds, UserInfo userInfo);

        Task<List<int>> GetBookingsForUpdate();
    }
}