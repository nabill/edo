using System;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public static class BookingExtensions
    {
        public static DateTime GetPayDueDate(this Data.Bookings.Booking booking)
            => (booking.DeadlineDate == null || booking.DeadlineDate?.DateTime == DateTime.MinValue ? booking.CheckInDate.DateTime : booking.DeadlineDate.Value.DateTime);


        public static MoneyAmount GetTotalPrice(this Data.Bookings.Booking booking) 
            => new(booking.TotalPrice, booking.Currency);
    }
}