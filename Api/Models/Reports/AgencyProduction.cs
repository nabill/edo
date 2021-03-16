namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct AgencyProduction
    {
        public string AgencyName { get; init; }
        public int TotalBookings { get; init; }
        public string Currency { get; init; }
        public decimal Revenue { get; init; }
        public int Nights { get; init; }
        public bool IsActive { get; init; }
    }
}