using System;
using System.Linq;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Reports.Helpers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.SupplierOptionsProvider;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class FinalizedBookingsReportDataConverter : IConverter<FinalizedBookingsReportData, FinalizedBookingsReportRow>
    {
        public FinalizedBookingsReportDataConverter(ISupplierOptionsStorage supplierOptionsStorage)
        {
            _supplierOptionsStorage = supplierOptionsStorage;
        }
        
        
        public FinalizedBookingsReportRow Convert(FinalizedBookingsReportData data)
        {
            var (_, isFailure, supplier, _) = _supplierOptionsStorage.Get(data.SupplierCode);
            var supplierName = isFailure
                ? string.Empty
                : supplier.Name;
            
            return new()
            {
                Created = DateTimeFormatters.ToDateString(data.Created),
                ReferenceCode = data.ReferenceCode,
                InvoiceNumber = data.InvoiceNumber,
                BookingStatus = EnumFormatters.FromDescription(data.BookingStatus),
                AgencyName = data.AgencyName,
                GuestName = data.GuestName,
                AccommodationName = data.AccommodationName,
                Rooms = string.Join("; ", data.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                ArrivalDate = DateTimeFormatters.ToDateString(data.ArrivalDate),
                DepartureDate = DateTimeFormatters.ToDateString(data.DepartureDate),
                ConfirmationNumber = data.ConfirmationNumber,
                RoomsConfirmationNumbers = string.Join("; ", data.Rooms.Select(r => r.SupplierRoomReferenceCode)),
                ConvertedAmount = data.SupplierConvertedPrice,
                ConvertedCurrency = data.SupplierConvertedCurrency,
                AmountExclVat = Math.Round(VatHelper.AmountExcludedVat(data.SupplierPrice), 2),
                VatAmount = Math.Round(VatHelper.VatAmount(data.SupplierPrice), 2),
                Supplier = supplierName,
                IsDirectContract = data.IsDirectContract ? "Yes" : "No",
                PayableByAgent = GetPayableByAgent(data),
                PayableByAgentCurrency = data.AgentCurrency,
                PayableToSupplierOrHotel = GetPayableToSupplierOrHotel(data),
                PayableToSupplierOrHotelCurrency = data.SupplierCurrency,
            };
        }


        private decimal GetPayableByAgent(FinalizedBookingsReportData data)
        {
            if (data.BookingStatus == BookingStatuses.Confirmed)
                return data.AgentPrice;

            return GetCancellationPenaltyAmount(data);
        }


        private decimal GetCancellationPenaltyAmount(FinalizedBookingsReportData data)
        {
            var booking = new Booking
            {
                Rooms = data.Rooms,
                CancellationPolicies = data.CancellationPolicies,
                Currency = data.AgentCurrency
            };

            return BookingCancellationPenaltyCalculator.Calculate(booking, data.CancellationDate.Value).Amount;
        }


        private decimal GetPayableToSupplierOrHotel(FinalizedBookingsReportData data)
        {
            // SupplierDeadline is required to properly calculate PayableToSupplierOrHotel
            // It was added in the beginning of summer 2021
            // Hence we cannot generate this report for earlier dates
            if (data.SupplierDeadline is null)
                throw new NotSupportedException($"The booking {data.ReferenceCode} has empty SupplierDeadline");
            
            if (data.BookingStatus == BookingStatuses.Confirmed)
                return data.SupplierPrice;

            if (data.AgentDeadline <= data.CancellationDate && data.CancellationDate <= data.SupplierDeadline.Date)
                return 0;

            var appliedPolicy = data.SupplierDeadline.Policies
                .OrderBy(policy => policy.FromDate)
                .LastOrDefault(policy => policy.FromDate <= data.CancellationDate);

            if (appliedPolicy is null)
                return data.SupplierPrice;
            
            var multiplier = (decimal) appliedPolicy.Percentage / 100;
            
            return data.SupplierPrice * multiplier;
        }


        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}