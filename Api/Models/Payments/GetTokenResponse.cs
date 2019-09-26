using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct GetTokenResponse
    {
        [JsonConstructor]
        public GetTokenResponse(string tokenId, PaymentTokenTypes type)
        {
            TokenId = tokenId;
            Type = type;
        }

        public string TokenId { get; }
        public PaymentTokenTypes Type { get; }
    }
}
