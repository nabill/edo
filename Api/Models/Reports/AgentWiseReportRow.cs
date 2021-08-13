namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct AgentWiseReportRow
    {
        public string Created { get; init; }
        public string ReferenceCode { get; init; }
        public string BookingStatus { get; init; }
        public string PaymentMethod { get; init; }
        public string GuestName { get; init; }
        public string AccommodationName { get; init; }
        public string Rooms { get; init; }
        public string ArrivalDate { get; init; }
        public string DepartureDate { get; init; }
        public double LenghtOfStay { get; init; }
        public decimal TotalPrice { get; init; }
        public string RoomsConfirmationNumbers { get; init; }
    }
}