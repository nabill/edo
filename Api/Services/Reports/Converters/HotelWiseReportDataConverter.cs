using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class HotelWiseReportDataConverter : IConverter<HotelWiseData, HotelWiseRow>
    {
        public HotelWiseRow Convert(HotelWiseData data, Func<decimal, decimal> vatAmountFunc, Func<decimal, decimal> amountExcludedVatFunc)
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