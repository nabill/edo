using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using Microsoft.AspNetCore.OData.Query;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IBookingService
    {
        Task<(int Count, IEnumerable<BookingSlim> Bookings)> GetAllBookings(ODataQueryOptions<BookingSlimProjection> opts);
        Task<(int Count, IEnumerable<BookingSlim> Bookings)> GetAgencyBookings(int agencyId, ODataQueryOptions<BookingSlimProjection> opts);
        Task<(int Count, IEnumerable<BookingSlim> Bookings)> GetAgentBookings(int agentId, ODataQueryOptions<BookingSlimProjection> opts);

    }
}