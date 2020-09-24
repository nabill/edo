using HappyTravel.Edo.Data.Booking;
using System;
using System.Linq;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class BookingExtensions
    {
        public static decimal GetRefundableAmount(this Booking booking, DateTime forDate) =>
            booking.Rooms.Sum(room => room.Price.Amount * (decimal) room.DeadlineDetails.GetRefundableFraction(forDate));
    }
}
