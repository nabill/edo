namespace HappyTravel.Edo.Data.Booking
{
    public readonly struct LocationInfo
    {
        public LocationInfo(string countryCode, string countryName, string cityCode, string cityName)
        {
            CountryCode = countryCode;
            CountryName = countryName;
            CityCode = cityCode;
            CityName = cityName;
        }
        
        public string CountryCode { get; }
        public string CountryName { get; }
        public string CityCode { get; }
        public string CityName { get; }
    }
}