using System.Collections.Generic;
using Newtonsoft.Json;

namespace Api.Models.Locations
{
    public readonly struct CountryRequest
    {
        [JsonConstructor]
        public CountryRequest(int marketId, List<string> countryCodes)
        {
            MarketId = marketId;
            CountryCodes = countryCodes;
        }


        public CountryRequest(int marketId, CountryRequest countryRequest) : this(marketId, countryRequest.CountryCodes)
        { }


        public int MarketId { get; }
        public List<string> CountryCodes { get; }
    }
}