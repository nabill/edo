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
        public static void AddBookingDetails(this Data.Booking.Booking booking, EdoContracts.Accommodations.Booking bookingDetails)
        {
            booking.DeadlineDate = bookingDetails.Deadline;
            booking.CheckInDate = bookingDetails.CheckInDate;
            booking.CheckOutDate = bookingDetails.CheckOutDate;
            booking.SupplierReferenceCode = bookingDetails.AgentReference;
            booking.Status = bookingDetails.Status;
            booking.UpdateMode = bookingDetails.BookingUpdateMode;

            booking.AddRoomsWithPassengers(bookingDetails.RoomContractSet.RoomContracts, bookingDetails.Rooms);
        }


        public static void AddRooms(this Data.Booking.Booking booking, List<RoomContract> roomContracts) => booking.AddRoomsWithPassengers(roomContracts, null);


        public static void AddRoomsWithPassengers(this Data.Booking.Booking booking, List<RoomContract> roomContracts, List<SlimRoomOccupationWithPrice> slimRoomDetailsWithPrices)
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
                        r.Deadline,
                        GetCorrespondingPassengers(number)))
                .ToList();


            List<Pax> GetCorrespondingPassengers(int number) =>
                slimRoomDetailsWithPrices == null
                    ? new List<Pax>()
                    : slimRoomDetailsWithPrices[number].RoomOccupation.Passengers;
        }
    }
}
