using System;
using System.Linq;
using System.Text.Json;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Reports.Helpers;
using HappyTravel.SupplierOptionsProvider;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class FullBookingsReportDataConverter : IConverter<FullBookingsReportData, FullBookingsReportRow>
    {
        public FullBookingsReportDataConverter(ISupplierOptionsStorage supplierOptionsStorage)
        {
            _supplierOptionsStorage = supplierOptionsStorage;
        }


        public FullBookingsReportRow Convert(FullBookingsReportData data)
        {
            var (_, isFailure, supplier, _) = _supplierOptionsStorage.Get(data.SupplierCode);
            var supplierName = isFailure
                ? string.Empty
                : supplier.Name;

            return new()
            {
                Created = DateTimeFormatters.ToDateString(data.Created),
                ReferenceCode = data.ReferenceCode,
                Status = data.Status,
                InvoiceNumber = data.InvoiceNumber,
                AgencyName = data.AgencyName,
                AgencyCity = data.AgencyCity,
                AgencyCountry = GetJsonProperty(data.AgencyCountry, "en"),
                AgencyMarket = data.AgencyMarket.GetValueOrDefault("en"),
                AgentName = data.AgentName,
                PaymentMethod = EnumFormatters.FromDescription(data.PaymentMethod),
                GuestName = data.GuestName,
                AccommodationName = data.AccommodationName,
                Rooms = string.Join("; ", data.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                ArrivalDate = DateTimeFormatters.ToDateString(data.ArrivalDate),
                DepartureDate = DateTimeFormatters.ToDateString(data.DepartureDate),
                ConfirmationNumber = data.ConfirmationNumber,
                RoomsConfirmationNumbers = string.Join("; ", data.Rooms.Select(r => r.SupplierRoomReferenceCode)),
                OriginalAmount = data.OriginalAmount,
                OriginalCurrency = data.OriginalCurrency,
                ConvertedAmount = data.ConvertedAmount,
                ConvertedCurrency = data.ConvertedCurrency,
                AmountExclVat = Math.Round(VatHelper.AmountExcludedVat(data.OriginalAmount), 2),
                VatAmount = Math.Round(VatHelper.VatAmount(data.OriginalAmount), 2),
                Supplier = supplierName,
                PaymentStatus = EnumFormatters.FromDescription(data.PaymentStatus)
            };
        }


        private string GetJsonProperty(JsonDocument jsonDocument, string property)
        {
            return jsonDocument.RootElement
                .GetProperty(property)
                .GetString();
        }


        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}