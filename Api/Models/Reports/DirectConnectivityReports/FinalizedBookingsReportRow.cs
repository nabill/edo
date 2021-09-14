using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports
{
    public readonly struct FinalizedBookingsReportRow
    {
        public string ReferenceCode { get; init; }
        public string InvoiceNumber { get; init; }
        public string BookingStatus { get; init; }
        public string AccommodationName { get; init; }
        public string Supplier { get; init; }
        public string ConfirmationNumber { get; init; }
        public string RoomsConfirmationNumbers { get; init; }
        public string AgencyName { get; init; }
        public string GuestName { get; init; }
        public string Created { get; init; }
        public string ArrivalDate { get; init; }
        public string DepartureDate { get; init; }
        public decimal ConvertedAmount { get; init; }
        public Currencies ConvertedCurrency { get; init; }
        public decimal AmountExclVat { get; init; }
        public decimal VatAmount { get; init; }
        public string Rooms { get; init; }
        public string IsDirectContract { get; init; }
        public decimal PayableToSupplierOrHotel { get; init; }
        public Currencies PayableToSupplierOrHotelCurrency { get; init; }
        public decimal PayableByAgent { get; init; }
        public Currencies PayableByAgentCurrency { get; init; }
    }
}