using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Booking;
using HappyTravel.Money.Extensions;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public static class BookingExtensions
    {
        public static List<Booking> FromEdoModels(this IEnumerable<Data.Bookings.Booking> bookings) 
            => bookings.Select(b => b.FromEdoModels()).ToList();


        public static List<SlimBooking> SlimFromEdoModels(this IEnumerable<Data.Bookings.Booking> bookings)
            => bookings.Select(b => new SlimBooking(clientReferenceCode: b.ClientReferenceCode,
                    referenceCode: b.ReferenceCode,
                    checkInDate: b.CheckInDate,
                    checkOutDate: b.CheckOutDate,
                    accommodationId: b.HtId,
                    totalPrice: b.TotalPrice.ToMoneyAmount(b.Currency),
                    isAdvancePurchaseRate: b.IsAdvancePurchaseRate,
                    status: b.Status,
                    mainPassengerName: GetMainPassengerName(b.Rooms)))
                .ToList();


        public static Booking FromEdoModels(this Data.Bookings.Booking booking)
        {
            return new Booking(clientReferenceCode: booking.ClientReferenceCode,
                referenceCode: booking.ReferenceCode,
                created: booking.Created,
                checkInDate: booking.CheckInDate,
                checkOutDate: booking.CheckOutDate,
                totalPrice: booking.TotalPrice.ToMoneyAmount(booking.Currency),
                status: booking.Status,
                rooms: booking.Rooms.FromEdoModel(),
                accommodationId: booking.HtId,
                cancellationPolicies: booking.CancellationPolicies
                    .Select(p => new CancellationPolicy(p.FromDate, p.Percentage))
                    .ToList(),
                cancelled: booking.Cancelled,
                isAdvancePurchaseRate: booking.IsAdvancePurchaseRate,
                isPackage: booking.IsPackage);
        }


        private static List<BookedRoom> FromEdoModel(this List<Data.Bookings.BookedRoom> bookedRooms)
        {
            return bookedRooms.Select(r => new BookedRoom(type: r.Type,
                price: r.Price,
                boardBasis: r.BoardBasis,
                mealPlan: r.MealPlan,
                contractDescription: r.ContractDescription,
                remarks: r.Remarks,
                deadlineDetails: new Deadline(date: r.DeadlineDetails.Date,
                    policies: r.DeadlineDetails.Policies?
                        .Select(p => new CancellationPolicy(p.FromDate, p.Percentage))
                        .ToList(),
                    remarks: r.DeadlineDetails.Remarks),
                passengers: r.Passengers)).ToList();
        }


        private static string GetMainPassengerName(List<Data.Bookings.BookedRoom> rooms)
        {
            var mainPassenger = rooms.SelectMany(r => r.Passengers)
                .FirstOrDefault(p => p.IsLeader);

            return mainPassenger is not null
                ? $"{mainPassenger.FirstName} {mainPassenger.LastName}"
                : string.Empty;
        }
    }
}