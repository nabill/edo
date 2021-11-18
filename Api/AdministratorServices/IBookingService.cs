using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Bookings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IBookingService
    {
        Task<Result<List<Booking>>> GetAgencyBookings(int agencyId);
        Task<Result<List<Booking>>> GetAgentBookings(int agentId);
    }
}
