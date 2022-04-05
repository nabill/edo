using HappyTravel.MultiLanguage;
using Newtonsoft.Json;

namespace Api.Models.Locations
{
    public readonly struct MarketRequest
    {
        [JsonConstructor]
        public MarketRequest(int marketId, MultiLanguage<string> names)
        {
            MarketId = marketId;
            Names = names;
        }


        public MarketRequest(int marketId, MarketRequest marketRequest) : this(marketId, marketRequest.Names)
        { }


        public int? MarketId { get; }
        public MultiLanguage<string> Names { get; }
    }
}