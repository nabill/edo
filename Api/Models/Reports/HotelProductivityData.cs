namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct HotelProductivityData
    {
        public string AccommodationName { get; init; }
        public int BookedNights { get; init; }
        public int BookedRooms { get; init; }
        public decimal TotalRevenue { get; init; }
    }
}