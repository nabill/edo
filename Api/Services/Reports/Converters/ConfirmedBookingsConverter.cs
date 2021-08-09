using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class ConfirmedBookingsConverter : IConverter<ConfirmedBookingsData, ConfirmedBookingsRow>
    {
        public ConfirmedBookingsRow Convert(ConfirmedBookingsData data)
            => new()
            {
                Created = data.Created,
                AccommodationName = data.AccommodationName,
                ReferenceCode = data.ReferenceCode,
                CheckInDate = data.CheckInDate,
                CheckOutDate = data.CheckOutDate,
                NumberOfPassengers = data.Rooms.Sum(room => room.Passengers.Count)
            };
    }
}