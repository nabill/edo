using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingStatusRefreshService : IBookingStatusRefreshService
    {
        public BookingStatusRefreshService(
            IDoubleFlow flow, 
            IDateTimeProvider dateTimeProvider, 
            IBookingManagementService bookingManagement,
            EdoContext context)
        {
            _flow = flow;
            _dateTimeProvider = dateTimeProvider;
            _bookingManagement = bookingManagement;
            _context = context;
        }


        public async Task<Result> RefreshStatus(Booking booking, UserInfo userInfo)
        {
            return await RefreshStatuses(new List<int> { booking.Id }, userInfo);
        }


        public async Task<Result<BatchOperationResult>> RefreshStatuses(List<int> bookingIds, UserInfo userInfo)
        {
            var states = await GetStates();
            var bookings = await _context.Bookings
                .Where(b => bookingIds.Contains(b.Id))
                .ToListAsync();

            return await ValidateCount()
                .Bind(ProcessBookings);
            
            
            Result ValidateCount()
                => bookings.Count != bookingIds.Count
                    ? Result.Failure("Invalid booking ids. Could not find some of requested bookings.")
                    : Result.Success();


            async Task<Result<BatchOperationResult>> ProcessBookings()
            {
                var builder = new StringBuilder();
                var hasErrors = false;
                
                foreach (var booking in bookings)
                {
                    var state = states.SingleOrDefault(s => s.BookingId == booking.Id);
                    var (_, isFailure, updatedState, error) = await RefreshStatus(booking, userInfo, state);

                    if (isFailure)
                    {
                        hasErrors = true;
                        builder.AppendLine(error);
                        continue;
                    }

                    UpdateState(updatedState);
                }

                await SaveStates();
                return new BatchOperationResult(builder.ToString(), hasErrors);
            }


            void UpdateState(BookingStatusRefreshState state)
            {
                var index = states.FindIndex(s => s.BookingId == state.BookingId);

                if (index >= 0)
                    states[index] = state;
                else
                    states.Add(state);
            }
            
            
            Task SaveStates()
            {
                states.RemoveAll(s => _dateTimeProvider.UtcNow() - s.LastRefreshingDate > Expiration);
                return _flow.SetAsync(Key, states, Expiration);
            }
        }


        public async Task<List<int>> GetBookingsForUpdate()
        {
            var states = await GetStates();

            var excludedIds = states
                .Where(s => !RefreshCondition(s, _dateTimeProvider.UtcNow()))
                .Select(s => s.BookingId)
                .ToList();

            return await _context.Bookings
                .Where(b => 
                    !excludedIds.Contains(b.Id) &&
                    BookingStatusesForRefresh.Contains(b.Status) && 
                    BookingUpdateModesForRefresh.Contains(b.UpdateMode))
                .Select(b => b.Id)
                .ToListAsync();
        }


        private async Task<Result<BookingStatusRefreshState>> RefreshStatus(Booking booking, UserInfo userInfo, BookingStatusRefreshState state)
        {
            return await ValidateBooking()
                .Bind(CheckIsRefreshStatusNeeded)
                .Bind(RefreshBookingStatus)
                .Map(GetUpdatedState);
            
            
            Result ValidateBooking()
            {
                if (!BookingStatusesForRefresh.Contains(booking.Status))
                    return Result.Failure<BookingStatusRefreshState>($"Cannot refresh booking status for booking {booking.ReferenceCode} with status {booking.Status}");
                
                if (!BookingUpdateModesForRefresh.Contains(booking.UpdateMode))
                    return Result.Failure<BookingStatusRefreshState>($"Cannot refresh booking status for booking {booking.ReferenceCode} with update mode {booking.UpdateMode}");

                return Result.Success();
            }


            Result CheckIsRefreshStatusNeeded()
            {
                if (state == default)
                    return Result.Success();
                
                return RefreshCondition(state, _dateTimeProvider.UtcNow())
                    ? Result.Success()
                    : Result.Failure($"Booking {booking.ReferenceCode} status is recently updated");
            }


            Task<Result> RefreshBookingStatus() 
                => _bookingManagement.RefreshStatus(booking, userInfo);


            BookingStatusRefreshState GetUpdatedState()
            {
                return state == default
                    ? new BookingStatusRefreshState
                    {
                        BookingId = booking.Id,
                        LastRefreshingDate = _dateTimeProvider.UtcNow()
                    }
                    : state with
                    {
                        LastRefreshingDate = _dateTimeProvider.UtcNow(),
                        RefreshStatusCount = state.RefreshStatusCount + 1
                    };
            }
        }


        private async Task<List<BookingStatusRefreshState>> GetStates()
        {
            return await _flow.GetAsync<List<BookingStatusRefreshState>>(Key, Expiration) 
                ?? new List<BookingStatusRefreshState>();
        }


        private static readonly HashSet<BookingStatuses> BookingStatusesForRefresh = new()
        {
            BookingStatuses.Pending
        };

        private static readonly HashSet<BookingUpdateModes> BookingUpdateModesForRefresh = new()
        {
            BookingUpdateModes.Synchronous
        };
        
        private static readonly Dictionary<int, TimeSpan> DelayStrategies = new()
        {
            {0, TimeSpan.FromSeconds(30)},
            {1, TimeSpan.FromSeconds(30)},
            {2, TimeSpan.FromSeconds(30)},
            {3, TimeSpan.FromSeconds(60)},
            {4, TimeSpan.FromSeconds(120)},
            {5, TimeSpan.FromSeconds(240)},
            {6, TimeSpan.FromSeconds(600)},
            {7, TimeSpan.FromSeconds(600)}
        };
        
        private static readonly TimeSpan DefaultDelayStrategy = TimeSpan.FromHours(1);

        private const string Key = "booking-status-refresh-states";

        private static readonly TimeSpan Expiration = TimeSpan.FromDays(3);

        private static readonly Func<BookingStatusRefreshState, DateTime, bool> RefreshCondition = (state, date) =>
        {
            var delay = DelayStrategies.TryGetValue(state.RefreshStatusCount, out var d) ? d : DefaultDelayStrategy;
            return state.LastRefreshingDate.Add(delay) < date;
        };


        private readonly IDoubleFlow _flow;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingManagementService _bookingManagement;
        private readonly EdoContext _context;
    }
}