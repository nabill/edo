using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class ConfirmedBookingsConverter : IConverter<ConfirmedBookingsProjection, ConfirmedBookingsRow>
    {
        public ConfirmedBookingsRow Convert(ConfirmedBookingsProjection projection, Func<decimal, decimal> vatAmountFunc,
            Func<decimal, decimal> amountExcludedVatFunc)
            => new()
            {
                Created = projection.Created,
                AccommodationName = projection.AccommodationName,
                ReferenceCode = projection.ReferenceCode,
                CheckInDate = projection.CheckInDate,
                CheckOutDate = projection.CheckOutDate,
                NumberOfPassengers = projection.Rooms.Sum(room => room.Passengers.Count)
            };
    }
}