using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Reports.Helpers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class SalesBookingsReportDataConverter : IConverter<SalesBookingsReportData, SalesBookingsReportRow>
    {
        public SalesBookingsReportRow Convert(SalesBookingsReportData data)
            => new()
            {
                Created = DateTimeFormatters.ToDateString(data.Created),
                ReferenceCode = data.ReferenceCode,
                InvoiceNumber = data.InvoiceNumber,
                BookingStatus = EnumFormatters.FromDescription(data.BookingStatus),
                AgencyName = data.AgencyName,
                PaymentMethod = EnumFormatters.FromDescription(data.PaymentMethod),
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
                Supplier = EnumFormatters.FromDescription(data.Supplier),
                PaymentStatus = EnumFormatters.FromDescription(data.PaymentStatus),
                IsDirectContract = data.IsDirectContract ? "Yes" : "No",
                PayableByAgent = GetPayableByAgent(data),
                PayableByAgentCurrency = data.AgentCurrency,
                PayableToSupplierOrHotel = GetPayableToSupplierOrHotel(data),
                PayableToSupplierOrHotelCurrency = data.SupplierCurrency,
            };


        private decimal GetPayableByAgent(SalesBookingsReportData data)
        {
            if (data.BookingStatus == BookingStatuses.Confirmed)
                return data.AgentPrice;

            return GetCancellationPenaltyAmount(data);
        }


        private decimal GetCancellationPenaltyAmount(SalesBookingsReportData data)
        {
            var booking = new Booking
            {
                Rooms = data.Rooms,
                CancellationPolicies = data.CancellationPolicies,
                Currency = data.AgentCurrency
            };

            return BookingCancellationPenaltyCalculator.Calculate(booking, data.CancellationDate.Value).Amount;
        }


        private decimal GetPayableToSupplierOrHotel(SalesBookingsReportData data)
        {
            if (data.SupplierDeadline is null)
                return 0;
            
            if (data.BookingStatus == BookingStatuses.Confirmed)
                return data.SupplierPrice;

            if (data.AgentDeadline <= data.CancellationDate && data.CancellationDate <= data.SupplierDeadline.Date)
                return 0;

            var appliedPolicy = data.SupplierDeadline.Policies
                .OrderBy(policy => policy.FromDate)
                .Last(policy => policy.FromDate <= data.CancellationDate);
            
            var multiplier = (decimal) appliedPolicy.Percentage / 100;
            
            return data.SupplierPrice * multiplier;
        }
    }
}