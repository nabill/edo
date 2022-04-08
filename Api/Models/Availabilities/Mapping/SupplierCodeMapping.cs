namespace HappyTravel.Edo.Api.Models.Availabilities.Mapping
{
    public readonly struct SupplierCodeMapping
    {
        public string AccommodationHtId { get; init; }
        public string SupplierCode { get; init; }
        public string LocalityHtId { get; init; }
        public string CountryHtId { get; init; }
        public int MarketId { get; init; }
    }
}