namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct AgencyProductivity
    {
        public string AgencyName { get; init; }
        public int BookingCount { get; init; }
        public string Currency { get; init; }
        public decimal Revenue { get; init; }
        public int NightCount { get; init; }
        public bool IsActive { get; init; }
        public string CountryName { get; init; }
        public string LocalityName { get; init; }
        public string AccommodationName { get; init; }
    }
}