using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Webhooks.Etg
{
    public readonly struct EtgBookingResponse
    {
        [JsonConstructor]
        public EtgBookingResponse(EtgBookingResponseData data, EtgBookingResponseSignature signature)
        {
            Data = data;
            Signature = signature;
        }
        
        
        [JsonProperty("data")]
        public EtgBookingResponseData Data { get; }
        
        [JsonProperty("signature")]
        public EtgBookingResponseSignature Signature { get; }
        
    }
}