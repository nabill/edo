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
            EdoContext context,
            IBookingRecordManager recordManager)
        {
            _flow = flow;
            _dateTimeProvider = dateTimeProvider;
            _bookingManagement = bookingManagement;
            _context = context;
            _recordManager = recordManager;
        }


        public async Task<Result> RefreshStatus(Booking booking, UserInfo userInfo, List<BookingStatusRefreshState> states = null)
        {
            states ??= await _flow.GetAsync<List<BookingStatusRefreshState>>(Key, Expiration) 
                ?? new List<BookingStatusRefreshState>();

            return await ValidateBooking()
                .Bind(CheckIsRefreshStatusNeeded)
                .Bind(RefreshBookingStatus)
                .Tap(CleanExpiredStates)
                .Tap(UpdateStates);
            
            
            Result ValidateBooking()
            {
                if (!BookingStatusesForRefresh.Contains(booking.Status))
                    return Result.Failure($"Cannot refresh booking status for booking {booking.ReferenceCode} with status {booking.Status}");
                
                if (!BookingUpdateModesForRefresh.Contains(booking.UpdateMode))
                    return Result.Failure($"Cannot refresh booking status for booking {booking.ReferenceCode} with update mode {booking.UpdateMode}");

                return Result.Success();
            }


            Result CheckIsRefreshStatusNeeded()
            {
                var state = states.SingleOrDefault(s => s.Id == booking.Id);
                
                if (state == default)
                    return Result.Success();
                
                return RefreshCondition(state, _dateTimeProvider.UtcNow())
                    ? Result.Success()
                    : Result.Failure($"Booking {booking.ReferenceCode} status is recently updated");
            }


            async Task<Result> RefreshBookingStatus()
            {
                var (_, isFailure, error) = await _bookingManagement.RefreshStatus(booking, userInfo);
                return isFailure
                    ? Result.Failure(error)
                    : Result.Success();
            }


            void CleanExpiredStates()
            {
                states.RemoveAll(s => _dateTimeProvider.UtcNow() - s.LastRefreshingDate > Expiration);
            }


            async Task UpdateStates()
            {
                var state = states.SingleOrDefault(s => s.Id == booking.Id);

                if (state == default)
                {
                    var stateEntry = new BookingStatusRefreshState
                    {
                        Id = booking.Id,
                        LastRefreshingDate = _dateTimeProvider.UtcNow()
                    };
                    states.Add(stateEntry);
                }
                else
                {
                    var stateEntry = state with
                    {
                        LastRefreshingDate = _dateTimeProvider.UtcNow(),
                        RefreshStatusCount = state.RefreshStatusCount + 1
                    };

                    var index = states.IndexOf(state);
                    states[index] = stateEntry;
                }

                await _flow.SetAsync(Key, states, Expiration);
            }
        }


        public async Task<Result<BatchOperationResult>> RefreshStatuses(List<int> bookingIds, UserInfo userInfo)
        {
            var builder = new StringBuilder();
            var hasErrors = false;
            var states = await _flow.GetAsync<List<BookingStatusRefreshState>>(Key, Expiration);

            foreach (var bookingId in bookingIds)
            {
                var (_, isFailure, error) = await RefreshStatus(bookingId, userInfo, states);

                if (isFailure)
                {
                    hasErrors = true;
                    builder.AppendLine(error);
                }
            }
            
            return new BatchOperationResult(builder.ToString(), hasErrors);
        }


        public async Task<List<int>> GetBookingsForUpdate()
        {
            var states = await _flow.GetAsync<List<BookingStatusRefreshState>>(Key, Expiration) 
                ?? new List<BookingStatusRefreshState>();

            var excludedIds = states
                .Where(s => !RefreshCondition(s, _dateTimeProvider.UtcNow()))
                .Select(s => s.Id)
                .ToList();

            return await _context.Bookings
                .Where(b => 
                    !excludedIds.Contains(b.Id) &&
                    BookingStatusesForRefresh.Contains(b.Status) && 
                    BookingUpdateModesForRefresh.Contains(b.UpdateMode))
                .Select(b => b.Id)
                .ToListAsync();
        }


        private Task<Result> RefreshStatus(int bookingId, UserInfo userInfo, List<BookingStatusRefreshState> states = null)
        {
            return _recordManager.Get(bookingId)
                .Bind(Refresh);

            Task<Result> Refresh(Booking booking) => RefreshStatus(booking, userInfo, states);
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
        private readonly IBookingRecordManager _recordManager;
    }
}