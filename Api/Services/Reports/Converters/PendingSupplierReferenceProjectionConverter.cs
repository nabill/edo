using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class PendingSupplierReferenceProjectionConverter : IConverter<PendingSupplierReferenceData, PendingSupplierReferenceRow>
    {
        public PendingSupplierReferenceRow Convert(PendingSupplierReferenceData data)
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