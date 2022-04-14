namespace HappyTravel.Edo.Api.Services.Markups.Abstractions
{
    public readonly struct MarkupDestinationInfo
    {
        public string CountryHtId { get; init; }
        public string LocalityHtId { get; init; }
        public string AccommodationHtId { get; init; }
        public int MarketId { get; init; }
    }
}