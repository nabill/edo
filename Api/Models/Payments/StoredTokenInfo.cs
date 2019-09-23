using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct StoredTokenInfo
    {
        [JsonConstructor]
        public StoredTokenInfo(string token, int customerId, PaymentTokenType tokenType, int? cardId)
        {
            Token = token;
            CustomerId = customerId;
            TokenType = tokenType;
            CardId = cardId;
        }

        public string Token { get; }
        public int CustomerId { get; }
        public PaymentTokenType TokenType { get; }
        public int? CardId { get; }
    }
}
