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
    public class BookingRefreshStatusService : IBookingRefreshStatusService
    {
        public BookingRefreshStatusService(
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


        public async Task<Result> RefreshStatus(Booking booking, UserInfo userInfo, List<BookingRefreshStatusState> states = null)
        {
            states ??= await _flow.GetAsync<List<BookingRefreshStatusState>>(Key, Expiration) 
                ?? new List<BookingRefreshStatusState>();

            return await ValidateBooking()
                .Bind(CheckIsRefreshStatusNeeded)
                .Bind(RefreshBookingStatus)
                .Tap(CleanExpiredStates)
                .Tap(UpdateStates);
            
            
            Result ValidateBooking()
            {
                if (!BookingStatusesForRefresh.Contains(booking.Status))
                    return Result.Failure($"Cannot refresh booking status for booking {booking.ReferenceCode}. Reason: {booking.Status} status is wrong");
                
                if (!BookingUpdateModesForRefresh.Contains(booking.UpdateMode))
                    return Result.Failure($"Cannot refresh booking status for booking {booking.ReferenceCode}. Reasons {booking.UpdateMode} update mode is wrong");

                return Result.Success();
            }


            Result CheckIsRefreshStatusNeeded()
            {
                var state = states.SingleOrDefault(s => s.Id == booking.Id);
                
                if (state == default)
                    return Result.Success();

                var delay = Delays.TryGetValue(state.RefreshStatusCount, out var d) ? d : DefaultDelay;
                return RefreshCondition(state, _dateTimeProvider.UtcNow(), delay)
                    ? Result.Success()
                    : Result.Failure($"Booking {booking.ReferenceCode} status updated on {state.LastRefreshingDate}. Next update time {state.LastRefreshingDate.Add(delay)}");
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
                    var stateEntry = new BookingRefreshStatusState
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
            var states = await _flow.GetAsync<List<BookingRefreshStatusState>>(Key, Expiration);
            
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
            var states = await _flow.GetAsync<List<BookingRefreshStatusState>>(Key, Expiration) 
                ?? new List<BookingRefreshStatusState>();

            var excludedIds = states
                .Where(s =>
                {
                    var delay = Delays.TryGetValue(s.RefreshStatusCount, out var d) ? d : DefaultDelay;
                    return !RefreshCondition(s, _dateTimeProvider.UtcNow(), delay);
                })
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


        private Task<Result> RefreshStatus(int bookingId, UserInfo userInfo, List<BookingRefreshStatusState> states = null)
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
        
        private static readonly Dictionary<int, TimeSpan> Delays = new()
        {
            {0, TimeSpan.Zero},
            {1, TimeSpan.FromSeconds(30)},
            {2, TimeSpan.FromSeconds(30)},
            {3, TimeSpan.FromSeconds(30)},
            {4, TimeSpan.FromSeconds(60)},
            {5, TimeSpan.FromSeconds(120)},
            {6, TimeSpan.FromSeconds(240)},
            {7, TimeSpan.FromSeconds(600)},
            {8, TimeSpan.FromSeconds(600)}
        };
        
        private static readonly TimeSpan DefaultDelay = TimeSpan.FromHours(1);

        private const string Key = "booking-refresh-status-states";

        private static readonly TimeSpan Expiration = TimeSpan.FromDays(3);

        private static readonly Func<BookingRefreshStatusState, DateTime, TimeSpan, bool> RefreshCondition = (state, date, delay)
            => state.LastRefreshingDate.Add(delay) > date;
        

        private readonly IDoubleFlow _flow;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingManagementService _bookingManagement;
        private readonly EdoContext _context;
        private readonly IBookingRecordManager _recordManager;
    }
}