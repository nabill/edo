using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Helpers;

namespace HappyTravel.Edo.Api.Services.Reports.Converters
{
    public class SalesBookingsReportDataConverter : IConverter<SalesBookingsReportData, SalesBookingsReportRow>
    {
        public SalesBookingsReportRow Convert(SalesBookingsReportData data, Func<decimal, decimal> vatAmountFunc,
            Func<decimal, decimal> amountExcludedVatFunc)
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
                AmountExclVat = Math.Round(amountExcludedVatFunc(data.OrderAmount), 2),
                VatAmount = Math.Round(vatAmountFunc(data.OrderAmount), 2),
                Supplier = EnumFormatters.FromDescription(data.Supplier),
                PaymentStatus = EnumFormatters.FromDescription(data.PaymentStatus),
                IsDirectContract = data.IsDirectContract ? "Yes" : "No",
                PayableByAgent = data.BookingStatus == BookingStatuses.Cancelled
                    ? GetCancellationPenaltyAmount(data, false)
                    : data.TotalPrice,
                PayableByAgentCurrency = data.TotalCurrency,
                PayableToSupplierOrHotel = data.BookingStatus == BookingStatuses.Cancelled
                    ? GetCancellationPenaltyAmount(data, true)
                    : data.ConvertedAmount,
                PayableToSupplierOrHotelCurrency = data.ConvertedCurrency,
            };


        private decimal GetCancellationPenaltyAmount(SalesBookingsReportData data, bool scaleToSupplierAmount)
        {
            if (data.CancellationDate is null)
                return 0m;

            var booking = new Booking
            {
                Rooms = data.Rooms,
                CancellationPolicies = data.CancellationPolicies,
                Currency = data.TotalCurrency
            };

            var amount = BookingCancellationPenaltyCalculator.Calculate(booking, data.CancellationDate.Value).Amount;

            if (scaleToSupplierAmount)
            {
                // Bookings are always in USD, ConvertedAmount too
                var multiplier = data.ConvertedAmount / data.TotalPrice;
                return MoneyRounder.Ceil(amount * multiplier, Currencies.USD);
            }

            return amount;
        }
    }
}