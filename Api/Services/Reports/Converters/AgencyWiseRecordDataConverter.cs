using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class AgencyWiseRecordDataConverter : IConverter<AgencyWiseRecordData, AgencyWiseReportRow>
    {
        public AgencyWiseReportRow Convert(AgencyWiseRecordData data)
            => new()
            {
                Date = DateTimeFormatters.ToDateString(data.Date),
                ReferenceCode = data.ReferenceCode,
                InvoiceNumber = data.InvoiceNumber,
                AgencyName = data.AgencyName,
                PaymentMethod = EnumFormatters.FromDescription(data.PaymentMethod),
                GuestName = data.GuestName,
                AccommodationName = data.AccommodationName,
                Rooms = string.Join("; ", data.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                ArrivalDate = DateTimeFormatters.ToDateString(data.ArrivalDate),
                DepartureDate = DateTimeFormatters.ToDateString(data.DepartureDate),
                LenghtOfStay = (data.DepartureDate - data.ArrivalDate).TotalDays,
                OriginalAmount = data.OriginalAmount,
                OriginalCurrency = data.OriginalCurrency,
                ConvertedAmount = data.ConvertedAmount,
                ConvertedCurrency = data.ConvertedCurrency,
                ConfirmationNumber = data.ConfirmationNumber,
                RoomsConfirmationNumbers = string.Join("; ", data.Rooms.Select(r => r.SupplierRoomReferenceCode)),
                PaymentStatus = EnumFormatters.FromDescription(data.PaymentStatus)
            };
    }
}