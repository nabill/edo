using Newtonsoft.Json;

namespace Api.Models.Locations
{
    public readonly struct MarketRequest
    {
        [JsonConstructor]
        public MarketRequest(int marketId, string name)
        {
            MarketId = marketId;
            Name = name;
        }


        public MarketRequest(int marketId, MarketRequest marketRequest) : this(marketId, marketRequest.Name)
        { }


        public int? MarketId { get; }
        public string Name { get; }
    }
}