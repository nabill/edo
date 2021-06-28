using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public  class SupplierWiseRecordProjectionConverter : IConverter<SupplierWiseRecordProjection, SupplierWiseReportRow>
    {
        public SupplierWiseReportRow Convert(SupplierWiseRecordProjection projection, Func<decimal, decimal> vatAmountFunc, Func<decimal, decimal> amountExcludedVatFunc) 
            => new()
            {
                ReferenceCode = projection.ReferenceCode,
                InvoiceNumber = projection.InvoiceNumber,
                AccommodationName = projection.AccommodationName,
                ConfirmationNumber = projection.ConfirmationNumber,
                RoomsConfirmationNumbers = string.Join("; ", projection.Rooms.Select(r => r.SupplierRoomReferenceCode)),
                RoomTypes = string.Join("; ", projection.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                GuestName = projection.GuestName ?? string.Empty,
                ArrivalDate = DateTimeFormatters.ToDateString(projection.ArrivalDate),
                DepartureDate = DateTimeFormatters.ToDateString(projection.DepartureDate),
                LenghtOfStay = (projection.DepartureDate - projection.ArrivalDate).TotalDays,
                AmountExclVat = Math.Round(amountExcludedVatFunc(projection.OriginalAmount), 2),
                VatAmount = Math.Round(vatAmountFunc(projection.OriginalAmount), 2),
                OriginalAmount = projection.OriginalAmount,
                OriginalCurrency = projection.OriginalCurrency,
                ConvertedAmount = projection.ConvertedAmount,
                ConvertedCurrency = projection.ConvertedCurrency,
                Supplier = EnumFormatters.FromDescription(projection.Supplier)
            };
    }
}