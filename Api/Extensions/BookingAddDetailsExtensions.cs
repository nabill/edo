using System;
using System.Linq;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class BookingAddDetailsExtensions
    {
        public static void AddBookingDetails(this Booking booking, BookingDetails bookingDetails)
        {
            booking.DeadlineDate = bookingDetails.Deadline;
            booking.CheckInDate = bookingDetails.CheckInDate;
            booking.CheckOutDate = bookingDetails.CheckOutDate;
            booking.SupplierReferenceCode = bookingDetails.AgentReference;
            booking.Status = bookingDetails.Status;
            booking.UpdateMode = bookingDetails.BookingUpdateMode;

            booking.Rooms = bookingDetails.RoomContractSet.RoomContracts
                .Select((r, number) =>
                {
                    var correspondingRoom = bookingDetails.RoomDetails[number].RoomDetails;
                    return new BookedRoom(r.Type,
                        r.IsExtraBedNeeded,
                        new MoneyAmount(r.TotalPrice.NetTotal, r.TotalPrice.Currency),
                        r.BoardBasis,
                        r.MealPlan,
                        r.DeadlineDate,
                        r.ContractDescription,
                        r.Remarks,
                        r.DeadlineDetails,
                        correspondingRoom.Passengers);
                })
                .ToList();
        }
    }
}
