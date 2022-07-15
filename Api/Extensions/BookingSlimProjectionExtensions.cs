using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Extensions;

public static class BookingSlimProjectionExtensions
{
    public static BookingSlim ToBookingSlim(this BookingSlimProjection projection)
    {
        return new BookingSlim
        {
            Id = projection.Id,
            ReferenceCode = projection.ReferenceCode,
            HtId = projection.HtId,
            AccommodationName = projection.AccommodationName,
            AgencyId = projection.AgencyId,
            AgentId = projection.AgentId,
            AgencyName = projection.AgencyName,
            AgentName = projection.AgentName,
            Created = projection.Created,
            Currency = projection.Currency,
            PaymentStatus = projection.PaymentStatus,
            PaymentType = projection.PaymentType,
            TotalPrice = projection.TotalPrice,
            CheckInDate = projection.CheckInDate,
            CheckOutDate = projection.CheckOutDate,
            DeadlineDate = projection.DeadlineDate,
            Status = projection.Status,
            Supplier = projection.Supplier,
            SupplierCode = projection.SupplierCode,
            CancellationDate = projection.CancellationDate,
            MainPassengerName = GetMainPassengerName(projection.Rooms),
            TotalPassengers = GetTotalPassengers(projection.Rooms)
        };
        
        string GetMainPassengerName(List<BookedRoom> rooms)
        {
            var room = rooms.FirstOrDefault();
            if (Equals(room, default))
                return "N/A";
            
            if (room.Passengers is null)
                return "N/A";

            var passenger = room.Passengers.FirstOrDefault(p => p.IsLeader);
            if (Equals(passenger, default))
                return "N/A";

            var title = passenger.Title.ToString();
            if (passenger.Title is PassengerTitles.Mr or PassengerTitles.Mrs or PassengerTitles.Ms)
                title += ".";
            
            return $"{title} {passenger.FirstName} {passenger.LastName}";
        }
        
        
        int GetTotalPassengers(List<BookedRoom> rooms) 
            => rooms.Where(room => room.Passengers is not null)
                .Sum(room => room.Passengers.Count);
    }
}