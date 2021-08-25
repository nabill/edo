using System.Linq;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Models.Reports;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class AgentWiseRecordDataConverter : IConverter<AgentWiseReportData, AgentWiseReportRow>
    {
        public AgentWiseReportRow Convert(AgentWiseReportData data) 
            => new()
            {
                Created = DateTimeFormatters.ToDateString(data.Created),
                ReferenceCode = data.ReferenceCode,
                PaymentMethod = EnumFormatters.FromDescription(data.PaymentMethod),
                GuestName = data.GuestName,
                AccommodationName = data.AccommodationName,
                Rooms = string.Join("; ", data.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                ArrivalDate = DateTimeFormatters.ToDateString(data.ArrivalDate),
                DepartureDate = DateTimeFormatters.ToDateString(data.DepartureDate),
                LenghtOfStay = (data.DepartureDate - data.ArrivalDate).TotalDays,
                RoomsConfirmationNumbers = string.Join("; ", data.Rooms.Select(r => r.SupplierRoomReferenceCode)),
                TotalPrice = data.TotalPrice,
                BookingStatus = data.Status.ToString()
            };
    }
}