using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.PropertyOwners;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.PropertyOwners
{
    public class BookingConfirmationService : IBookingConfirmationService
    {
        public BookingConfirmationService(EdoContext context, IBookingRecordsUpdater recordsUpdater)
        {
            _context = context;
            _recordsUpdater = recordsUpdater;
        }


        public async Task<Result> Update(BookingConfirmation bookingConfirmation)
        {
            return await GetBooking()
                .Ensure(IsDirectContract, $"Booking with the reference code '{bookingConfirmation.ReferenceCode}' is not a direct contract")
                .BindWithTransaction(_context, booking => UpdateBooking(booking)
                    .Tap(SendStatusToPms)
                    .Tap(SaveHistory));


            async Task<Result<Booking>> GetBooking()
            {
                var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == bookingConfirmation.ReferenceCode);

                return booking is null
                    ? Result.Failure<Booking>($"Booking with the reference code '{bookingConfirmation.ReferenceCode}' is not found")
                    : booking;
            }


            bool IsDirectContract(Booking booking)
                => booking.IsDirectContract;


            async Task<Result> UpdateBooking(Booking booking)
            {
                if (bookingConfirmation.ConfirmationCode != string.Empty)
                {
                    booking.ConfirmationCode = bookingConfirmation.ConfirmationCode;
                    _context.Bookings.Update(booking);
                    await _context.SaveChangesAsync();
                }

                var newStatus = bookingConfirmation.Status switch
                {
                    BookingConfirmationStatuses.OnRequest => BookingStatuses.Pending,
                    BookingConfirmationStatuses.Amended => BookingStatuses.ManualCorrectionNeeded,
                    BookingConfirmationStatuses.Confirmed => BookingStatuses.Confirmed,
                    BookingConfirmationStatuses.Cancelled => BookingStatuses.Cancelled,
                    _ => throw new NotImplementedException()
                };

                return await _recordsUpdater.ChangeStatus(booking: booking, 
                    status: newStatus,
                    date: bookingConfirmation.CreatedAt, 
                    apiCaller: Models.Users.ApiCaller.InternalServiceAccount, 
                    reason: new BookingChangeReason 
                    {
                        Source = BookingChangeSources.Hotel,
                        Event = BookingChangeEvents.HotelConfirmation,
                        Reason = $"Status changed by property owner employee {bookingConfirmation.Initiator}"
                    });
            }


            Task SendStatusToPms()
            {
                // TODO: Sending the hotel's changed booking status to PMS (Columbus) will be implemented in task AA-4xx
                return Task.CompletedTask;
            }


            Task SaveHistory()
            {
                _context.HotelConfirmationHistory.Add(new HotelConfirmationHistoryEntry
                {
                    ReferenceCode = bookingConfirmation.ReferenceCode,
                    ConfirmationCode = bookingConfirmation.ConfirmationCode,
                    Status = bookingConfirmation.Status,
                    Initiator = bookingConfirmation.Initiator,
                    CreatedAt = bookingConfirmation.CreatedAt
                });

                return _context.SaveChangesAsync();
            }
        }


        private readonly EdoContext _context;
        private readonly IBookingRecordsUpdater _recordsUpdater;
    }
}