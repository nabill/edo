using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Hotels;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Hotel
{
    public class HotelConfirmationService : IHotelConfirmationService
    {
        public HotelConfirmationService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result> Update(HotelConfirmation hotelConfirmation)
        {
            return await GetBooking()
                .Ensure(IsDirectContract, $"Booking with reference code '{hotelConfirmation.ReferenceCode}' is not a direct contract")
                .BindWithTransaction(_context, booking => UpdateBooking(booking)
                    .Tap(SendStatusToPMS)
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
                if (hotelConfirmation.ReferenceCode != string.Empty)
                    booking.ReferenceCode = hotelConfirmation.ReferenceCode;
                booking.Status = hotelConfirmation.Status switch
                {
                    HotelConfirmationStatuses.OnRequest => BookingStatuses.Pending,
                    HotelConfirmationStatuses.Amended => BookingStatuses.ManualCorrectionNeeded,
                    HotelConfirmationStatuses.Confirmed => BookingStatuses.Confirmed,
                    HotelConfirmationStatuses.Cancelled => BookingStatuses.Cancelled,
                    _ => throw new NotImplementedException()
                };
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                return Result.Success();
            }


            Task SendStatusToPMS()
            {
                // TODO: Sending the hotel's changed booking status to PMS (Columbus) will be implemented in task AA-4
                return Task.CompletedTask;
            }


            Task SaveHistory()
            {
                _context.HotelConfirmationHistory.Add(new HotelConfirmationHistoryEntry
                {
                    ReferenceCode = hotelConfirmation.ReferenceCode,
                    Status = hotelConfirmation.Status,
                    Initiator = hotelConfirmation.Initiator,
                    CreatedAt = hotelConfirmation.CreatedAt
                });

                return _context.SaveChangesAsync();
            }
        }


        private readonly EdoContext _context;
    }
}