using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Hotels;
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


        public async Task<Result> Update(BookingConfirmation hotelConfirmation)
        {
            return await GetBooking()
                .Ensure(IsDirectContract, $"Booking with reference code '{hotelConfirmation.ReferenceCode}' is not a direct contract")
                .BindWithTransaction(_context, booking => UpdateBooking(booking)
                    .Tap(SendStatusToPms)
                    .Tap(SaveHistory));


            async Task<Result<Booking>> GetBooking()
            {
                var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == hotelConfirmation.ReferenceCode);

                return booking is null
                    ? Result.Failure<Booking>($"Booking with reference code '{hotelConfirmation.ReferenceCode}' not found")
                    : booking;
            }


            bool IsDirectContract(Booking booking)
                => booking.IsDirectContract;


            async Task<Result> UpdateBooking(Booking booking)
            {
                if (hotelConfirmation.ConfirmationCode != string.Empty)
                {
                    booking.ConfirmationCode = hotelConfirmation.ConfirmationCode;
                    _context.Bookings.Update(booking);
                    await _context.SaveChangesAsync();
                }

                var newStatus = hotelConfirmation.Status switch
                {
                    BookingConfirmationStatuses.OnRequest => BookingStatuses.Pending,
                    BookingConfirmationStatuses.Amended => BookingStatuses.ManualCorrectionNeeded,
                    BookingConfirmationStatuses.Confirmed => BookingStatuses.Confirmed,
                    BookingConfirmationStatuses.Cancelled => BookingStatuses.Cancelled,
                    _ => throw new NotImplementedException()
                };

                return await _recordsUpdater.ChangeStatus(booking: booking, 
                    status: newStatus,
                    date: hotelConfirmation.CreatedAt, 
                    apiCaller: Models.Users.ApiCaller.InternalServiceAccount, 
                    reason: new BookingChangeReason 
                    {
                        Source = BookingChangeSources.Hotel,
                        Event = BookingChangeEvents.HotelConfirmation,
                        Reason = $"Status changed by hotel employee {hotelConfirmation.Initiator}"
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
                    ReferenceCode = hotelConfirmation.ReferenceCode,
                    ConfirmationCode = hotelConfirmation.ConfirmationCode,
                    Status = hotelConfirmation.Status,
                    Initiator = hotelConfirmation.Initiator,
                    CreatedAt = hotelConfirmation.CreatedAt
                });

                return _context.SaveChangesAsync();
            }
        }


        private readonly EdoContext _context;
        private readonly IBookingRecordsUpdater _recordsUpdater;
    }
}