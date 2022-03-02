using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Reports.Helpers;
using HappyTravel.SupplierOptionsProvider;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public  class PayableToSupplierRecordDataConverter : IConverter<PayableToSupplierRecordData, PayableToSupplierReportRow>
    {
        public PayableToSupplierRecordDataConverter(ISupplierOptionsStorage supplierOptionsStorage)
        {
            _supplierOptionsStorage = supplierOptionsStorage;
        }
        
        
        public PayableToSupplierReportRow Convert(PayableToSupplierRecordData data) 
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
                AmountExclVat = Math.Round(VatHelper.AmountExcludedVat(data.OriginalAmount), 2),
                VatAmount = Math.Round(VatHelper.VatAmount(data.OriginalAmount), 2),
                OriginalAmount = data.OriginalAmount,
                OriginalCurrency = data.OriginalCurrency,
                ConvertedAmount = data.ConvertedAmount,
                ConvertedCurrency = data.ConvertedCurrency,
                Supplier = _supplierOptionsStorage.Get(data.SupplierCode).Name
            };


        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}