using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Reports.Helpers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;

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
                ConvertedAmount = data.ConvertedAmount,
                ConvertedCurrency = data.ConvertedCurrency,
                AmountExclVat = Math.Round(VatHelper.AmountExcludedVat(data.OrderAmount), 2),
                VatAmount = Math.Round(VatHelper.VatAmount(data.OrderAmount), 2),
                Supplier = EnumFormatters.FromDescription(data.Supplier),
                PaymentStatus = EnumFormatters.FromDescription(data.PaymentStatus),
                IsDirectContract = data.IsDirectContract ? "Yes" : "No",
                PayableByAgent = data.BookingStatus == BookingStatuses.Cancelled
                    ? GetCancellationPenaltyAmount(data)
                    : data.TotalPrice,
                PayableByAgentCurrency = data.TotalCurrency,
                PayableToSupplierOrHotel = GetPayableToSupplierOrHotel(data),
                PayableToSupplierOrHotelCurrency = data.ConvertedCurrency,
            };


        private decimal GetCancellationPenaltyAmount(SalesBookingsReportData data)
        {
            if (data.CancellationDate is null)
                return 0m;

            var booking = new Booking
            {
                Rooms = data.Rooms,
                CancellationPolicies = data.CancellationPolicies,
                Currency = data.TotalCurrency
            };

            return BookingCancellationPenaltyCalculator.Calculate(booking, data.CancellationDate.Value).Amount;
        }


        private decimal GetPayableToSupplierOrHotel(SalesBookingsReportData data)
        {
            var penaltyAmount = GetCancellationPenaltyAmount(data);
            var multiplier = data.ConvertedAmount / data.TotalPrice;
            var scaledAmount = MoneyRounder.Ceil(penaltyAmount * multiplier, Currencies.USD); // Bookings are always in USD, ConvertedAmount too
            
            return data.BookingStatus == BookingStatuses.Cancelled
                ? data.CancellationDate >= data.AgentDeadline && data.CancellationDate <= data.SupplierDeadline 
                    ? 0
                    : scaledAmount
                : data.ConvertedAmount;
        }
    }
}