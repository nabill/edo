using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct GetTokenRequest
    {
        [JsonConstructor]
        public GetTokenRequest(int cardId)
        {
            CardId = cardId;
        }

        public int CardId { get; }
    }
}
