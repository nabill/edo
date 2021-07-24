using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class SalesBookingsReportProjectionConverter : IConverter<SalesBookingsReportProjection, SalesBookingsReportRow>
    {
        public SalesBookingsReportRow Convert(SalesBookingsReportProjection projection, Func<decimal, decimal> vatAmountFunc,
            Func<decimal, decimal> amountExcludedVatFunc)
            => new()
            {
                Created = DateTimeFormatters.ToDateString(projection.Created),
                ReferenceCode = projection.ReferenceCode,
                InvoiceNumber = projection.InvoiceNumber,
                BookingStatus = EnumFormatters.FromDescription(projection.BookingStatus),
                AgencyName = projection.AgencyName,
                PaymentMethod = EnumFormatters.FromDescription(projection.PaymentMethod),
                GuestName = projection.GuestName,
                AccommodationName = projection.AccommodationName,
                Rooms = string.Join("; ", projection.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                ArrivalDate = DateTimeFormatters.ToDateString(projection.ArrivalDate),
                DepartureDate = DateTimeFormatters.ToDateString(projection.DepartureDate),
                ConfirmationNumber = projection.ConfirmationNumber,
                RoomsConfirmationNumbers = string.Join("; ", projection.Rooms.Select(r => r.SupplierRoomReferenceCode)),
                ConvertedAmount = projection.ConvertedAmount,
                ConvertedCurrency = projection.ConvertedCurrency,
                AmountExclVat = Math.Round(amountExcludedVatFunc(projection.OrderAmount), 2),
                VatAmount = Math.Round(vatAmountFunc(projection.OrderAmount), 2),
                Supplier = EnumFormatters.FromDescription(projection.Supplier),
                PaymentStatus = EnumFormatters.FromDescription(projection.PaymentStatus),
                IsDirectContract = projection.IsDirectContract ? "Yes" : "No",
                PayableByAgent = projection.BookingStatus == BookingStatuses.Cancelled
                    ? GetCancellationPenaltyAmount(projection, false)
                    : projection.TotalPrice,
                PayableByAgentCurrency = projection.TotalCurrency,
                PayableToSupplierOrHotel = projection.BookingStatus == BookingStatuses.Cancelled
                    ? GetCancellationPenaltyAmount(projection, true)
                    : projection.ConvertedAmount,
                PayableToSupplierOrHotelCurrency = projection.ConvertedCurrency,
            };


        private decimal GetCancellationPenaltyAmount(SalesBookingsReportProjection projection, bool scaleToSupplierAmount)
        {
            if (projection.CancellationDate is null)
                return 0m;

            var booking = new Booking
            {
                Rooms = projection.Rooms,
                CancellationPolicies = projection.CancellationPolicies,
                Currency = projection.TotalCurrency
            };

            var amount = BookingCancellationPenaltyCalculator.Calculate(booking, projection.CancellationDate.Value).Amount;

            if (scaleToSupplierAmount)
            {
                // Bookings are always in USD, ConvertedAmount too
                var multiplier = projection.ConvertedAmount / projection.TotalPrice;
                return amount * multiplier;
            }

            return amount;
        }
    }
}