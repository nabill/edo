using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HappyTravel.Edo.Data;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class BookingService : IBookingService
    {
        public BookingService(EdoContext context, ISupplierOptionsStorage supplierOptionsStorage)
        {
            _context = context;
            _supplierOptionsStorage = supplierOptionsStorage;
        }


        public Task<(int Count, IEnumerable<BookingSlim> Bookings)> GetAllBookings(ODataQueryOptions<BookingSlimProjection> opts)
            => GetBookings(opts);


        public Task<(int Count, IEnumerable<BookingSlim> Bookings)> GetAgencyBookings(int agencyId, ODataQueryOptions<BookingSlimProjection> opts)
            => GetBookings(opts, booking => booking.AgencyId == agencyId);

        
        public Task<(int Count, IEnumerable<BookingSlim> Bookings)> GetAgentBookings(int agentId, ODataQueryOptions<BookingSlimProjection> opts)
            => GetBookings(opts, booking => booking.AgentId == agentId);


        private async Task<(int Count, IEnumerable<BookingSlim> Bookings)> GetBookings(ODataQueryOptions<BookingSlimProjection> opts, Expression<Func<BookingSlimProjection, bool>>? expression = null)
        {
            var (_, isFailure, suppliers, _) = _supplierOptionsStorage.GetAll();
            var suppliersDictionary = isFailure
                ? new Dictionary<string, string>(0)
                : suppliers.ToDictionary(s => s.Code, s => s.Name);
            
            var query = from booking in _context.Bookings
                    join agent in _context.Agents on booking.AgentId equals agent.Id
                    join agency in _context.Agencies on booking.AgencyId equals agency.Id
                select new BookingSlimProjection
                {
                    Id = booking.Id,
                    ReferenceCode = booking.ReferenceCode,
                    HtId = booking.HtId,
                    AccommodationName = booking.AccommodationName,
                    AgencyId = booking.AgencyId,
                    AgentId = booking.AgentId,
                    AgencyName = agency.Name,
                    AgentName = $"{agent.FirstName} {agent.LastName}",
                    Created = booking.Created,
                    Currency = booking.Currency,
                    PaymentStatus = booking.PaymentStatus,
                    PaymentType = booking.PaymentType,
                    TotalPrice = booking.TotalPrice,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    DeadlineDate = booking.DeadlineDate,
                    Status = booking.Status,
                    Supplier = suppliersDictionary[booking.SupplierCode],
                    SupplierCode = booking.SupplierCode,
                    CancellationDate = booking.Cancelled,
                    MainPassengerName = booking.MainPassengerName,
                    Rooms = booking.Rooms
                };

            if (expression != null)
                query = query.Where(expression);
            
            var countQuery = opts.ApplyTo(query, AllowedQueryOptions.Skip | AllowedQueryOptions.Top);
            var totalCount = await countQuery.Cast<BookingSlimProjection>().CountAsync();

            var dataQuery = opts.ApplyTo(query);
            var dataResult = await dataQuery.Cast<BookingSlimProjection>().ToListAsync();

            var bookings = dataResult.Select(projection => projection.ToBookingSlim());

            return (totalCount, bookings);
        }


        private readonly EdoContext _context;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}
