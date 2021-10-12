using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusEmbedded
    {
        [JsonProperty("payment")]
        public List<NGeniusPayment> Payments { get; init; }
    }
}