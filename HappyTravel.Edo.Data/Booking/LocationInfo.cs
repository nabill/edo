namespace HappyTravel.Edo.Data.Booking
{
    public readonly struct LocationInfo
    {
        public LocationInfo(string country, string locality, string zone)
        {
            Country = country;
            Locality = locality;
            Zone = zone;
        }
        
        public string Country { get; }
        public string Locality { get; }
        public string Zone { get; }
    }
}