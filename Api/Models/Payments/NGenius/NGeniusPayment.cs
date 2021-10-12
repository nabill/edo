using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusPayment
    {
        [JsonProperty("_id")]
        public string Id { get; init; }
        
        public string State { get; init; }
        public string OrderReference { get; init; }
        public string MerchantOrderReference { get; init; }
    }
}