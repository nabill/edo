using System;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public static class BookingExtensions
    {
        public static DateTime GetPayDueDate(this Data.Bookings.Booking booking)
            => (booking.DeadlineDate ?? booking.CheckInDate)
                .AddDays(-BookingConstants.DaysBeforeDeadlineWhenPayForBooking);
    }
}