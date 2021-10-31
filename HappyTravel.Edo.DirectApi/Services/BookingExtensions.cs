using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Money.Extensions;

namespace HappyTravel.Edo.DirectApi.Services
{
    public static class BookingExtensions
    {
        public static Booking FromEdoModels(this Data.Bookings.Booking booking)
        {
            return new Booking(referenceCode: booking.ReferenceCode,
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