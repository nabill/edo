namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct VccBookingRow
    {
        public string GuestName { get; init; }
        public string ReferenceCode { get; init; }
        public string CheckInDate { get; init; }
        public string CheckOutDate { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; }
        public string CardActivationDate { get; init; }
        public string CardDueDate { get; init; }
        public string CardNumber { get; init; }
        public decimal CardAmount { get; init; }
    }
}