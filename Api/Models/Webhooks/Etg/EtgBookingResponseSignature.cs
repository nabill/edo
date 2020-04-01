using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Webhooks.Etg
{
    public readonly struct EtgBookingResponseSignature
    {
        [JsonConstructor]
        public EtgBookingResponseSignature(string signature, int timestamp, string token )
        {
            Signature = signature;
            Timestamp = timestamp;
            Token = token;
        }
        
        
        [JsonProperty("signature")]
        public string Signature { get; }
        
        [JsonProperty("timestamp")]
        public long Timestamp { get; }
        
        [JsonProperty("token")]
        public string Token { get; }
    }
}