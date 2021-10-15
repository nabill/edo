using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusOrder
    {
        [JsonProperty("_id")]
        public string Id { get; init; }
        
        // TODO: add Links
        
        public string Action { get; init; }
        public NGeniusAmount Amount { get; init; }
        public string Language { get; init; }
        
        
        // TODO: add merchant attributes
        
        public string EmailAddress { get; init; }
        public string Reference { get; init; }
        public string OutletId { get; init; }
        
        [JsonProperty("createDateTime")]
        public DateTime Created { get; init; }
        
        // TODO: add payment methods
        
        public string Referrer { get; init; }
        
        // TODO: add formattedAmount and formattedOrderSummary
        
        [JsonProperty("_embedded")]
        public NGeniusEmbedded Embedded { get; init; }
    }
}