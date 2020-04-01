using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Webhooks.Etg
{
    public readonly struct EtgBookingResponseData
    {
        [JsonConstructor]
        public EtgBookingResponseData(string partnerOrderId, string status)
        {
            PartnerOrderId = partnerOrderId;
            Status = status;
        }
        
        
        [JsonProperty("partner_order_id")]
        public string PartnerOrderId { get; }
        
        [JsonProperty("status")]
        public string Status { get; }
    }
}