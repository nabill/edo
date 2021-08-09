using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class HotelWiseBookingReportDataConverter : IConverter<HotelWiseData, HotelWiseRow>
    {
        public HotelWiseRow Convert(HotelWiseData data)
            => new()
            {
                Created = data.Created,
                BookingStatus = data.BookingStatus.ToString(),
                CheckInDate = data.CheckInDate,
                CheckOutDate = data.CheckOutDate,
                NumberOfPassengers = data.Rooms.Sum(x => x.Passengers.Count),
                AccommodationName = data.AccommodationName,
                ReferenceCode = data.ReferenceCode
            };
    }
}