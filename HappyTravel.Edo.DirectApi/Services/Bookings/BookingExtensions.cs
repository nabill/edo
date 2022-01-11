﻿using System.Collections.Generic;
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
                deadlineDetails: r.DeadlineDetails,
                passengers: r.Passengers)).ToList();
        }
    }
}