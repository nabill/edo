using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public  class SupplierWiseRecordDataConverter : IConverter<SupplierWiseRecordData, SupplierWiseReportRow>
    {
        public SupplierWiseReportRow Convert(SupplierWiseRecordData data, Func<decimal, decimal> vatAmountFunc, Func<decimal, decimal> amountExcludedVatFunc) 
            => new()
            {
                ReferenceCode = data.ReferenceCode,
                InvoiceNumber = data.InvoiceNumber,
                AccommodationName = data.AccommodationName,
                ConfirmationNumber = data.ConfirmationNumber,
                RoomsConfirmationNumbers = string.Join("; ", data.Rooms.Select(r => r.SupplierRoomReferenceCode)),
                RoomTypes = string.Join("; ", data.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                GuestName = data.GuestName ?? string.Empty,
                ArrivalDate = DateTimeFormatters.ToDateString(data.ArrivalDate),
                DepartureDate = DateTimeFormatters.ToDateString(data.DepartureDate),
                LenghtOfStay = (data.DepartureDate - data.ArrivalDate).TotalDays,
                AmountExclVat = Math.Round(amountExcludedVatFunc(data.OriginalAmount), 2),
                VatAmount = Math.Round(vatAmountFunc(data.OriginalAmount), 2),
                OriginalAmount = data.OriginalAmount,
                OriginalCurrency = data.OriginalCurrency,
                ConvertedAmount = data.ConvertedAmount,
                ConvertedCurrency = data.ConvertedCurrency,
                Supplier = EnumFormatters.FromDescription(data.Supplier)
            };
    }
}