namespace HappyTravel.Edo.Api.Services.Markups.Abstractions
{
    public readonly struct MarkupObjectInfo
    {
        public MarkupObjectInfo(string countryHtId, string localityHtId, string accommodationHtId)
        {
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
            AccommodationHtId = accommodationHtId;
        }
        
        public string CountryHtId { get; }
        public string LocalityHtId { get; }
        public string AccommodationHtId { get; }
    }
}