using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Money.Extensions;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public static class BookingExtensions
    {
        public static List<Booking> FromEdoModels(this IEnumerable<Data.Bookings.Booking> bookings) 
            => bookings.Select(b => b.FromEdoModels()).ToList();


        public static Booking FromEdoModels(this Data.Bookings.Booking booking)
        {
            return new Booking(referenceCode: booking.ClientReferenceCode,
                supplierReferenceCode: booking.ReferenceCode,
                created: booking.Created,
                checkInDate: booking.CheckInDate,
                checkOutDate: booking.CheckOutDate,
                deadlineDate: booking.DeadlineDate.Value,
                totalPrice: booking.TotalPrice.ToMoneyAmount(booking.Currency),
                status: booking.Status,
                rooms: booking.Rooms,
                accommodationId: booking.HtId,
                cancellationPolicies: booking.CancellationPolicies,
                cancelled: booking.Cancelled,
                isAdvancePurchaseRate: booking.IsAdvancePurchaseRate,
                isPackage: booking.IsPackage);
        }
    }
}