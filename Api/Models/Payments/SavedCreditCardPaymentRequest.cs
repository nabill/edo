using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct SavedCreditCardPaymentRequest
    {
        [JsonConstructor]
        public SavedCreditCardPaymentRequest(int cardId, string referenceCode, string securityCode)
        {
            CardId = cardId;
            ReferenceCode = referenceCode;
            SecurityCode = securityCode;
        }


        public int CardId { get; }

        public string ReferenceCode { get; }

        public string SecurityCode { get; }
    }
}