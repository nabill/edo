using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class BookingExtensions
    {
        public static void AddBookingDetails(this Booking booking, BookingDetails bookingDetails)
        {
            booking.DeadlineDate = bookingDetails.Deadline;
            booking.CheckInDate = bookingDetails.CheckInDate;
            booking.CheckOutDate = bookingDetails.CheckOutDate;
            booking.SupplierReferenceCode = bookingDetails.AgentReference;
            booking.Status = bookingDetails.Status;
            booking.UpdateMode = bookingDetails.BookingUpdateMode;

            booking.AddRoomsWithPassengers(bookingDetails.RoomContractSet.RoomContracts, bookingDetails.RoomDetails);
        }


        public static void AddRooms(this Booking booking, List<RoomContract> roomContracts) => booking.AddRoomsWithPassengers(roomContracts, null);


        public static void AddRoomsWithPassengers(this Booking booking, List<RoomContract> roomContracts, List<SlimRoomDetailsWithPrice> slimRoomDetailsWithPrices)
        {
            booking.Rooms = roomContracts
                .Select((r, number) =>
                    new BookedRoom(r.Type,
                        r.IsExtraBedNeeded,
                        new MoneyAmount(r.TotalPrice.NetTotal, r.TotalPrice.Currency),
                        r.BoardBasis,
                        r.MealPlan,
                        r.DeadlineDate,
                        r.ContractDescription,
                        r.Remarks,
                        r.DeadlineDetails,
                        GetCorrespondingPassengers(number)))
                .ToList();


            List<Pax> GetCorrespondingPassengers(int number) =>
                slimRoomDetailsWithPrices == null
                    ? new List<Pax>()
                    : slimRoomDetailsWithPrices[number].RoomDetails.Passengers;
        }
    }
}
