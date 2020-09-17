using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class BookingExtensions
    {
        public static void AddBookingDetails(this Booking booking, EdoContracts.Accommodations.Booking bookingDetails)
        {
            booking.DeadlineDate = bookingDetails.Deadline;
            booking.CheckInDate = bookingDetails.CheckInDate;
            booking.CheckOutDate = bookingDetails.CheckOutDate;
            booking.SupplierReferenceCode = bookingDetails.AgentReference;
            booking.Status = bookingDetails.Status;
            booking.UpdateMode = bookingDetails.BookingUpdateMode;

            booking.AddPassengersToRooms(bookingDetails.Rooms);
        }


        public static void AddRooms(this Booking booking, List<RoomContract> roomContracts)
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
                        new List<Pax>()))
                .ToList();
        }


        private static void AddPassengersToRooms(this Booking booking, List<SlimRoomOccupationWithPrice> slimRoomDetailsWithPrices)
        {
            booking.Rooms = booking.Rooms
                .Select((r, number) =>
                    new BookedRoom(r.Type,
                        r.IsExtraBedNeeded,
                        r.Price,
                        r.BoardBasis,
                        r.MealPlan,
                        r.DeadlineDate,
                        r.ContractDescription,
                        r.Remarks,
                        r.DeadlineDetails,
                        GetCorrespondingPassengers(number)))
                .ToList();


            List<Pax> GetCorrespondingPassengers(int number) => slimRoomDetailsWithPrices[number].RoomOccupation.Passengers;
        }
    }
}
