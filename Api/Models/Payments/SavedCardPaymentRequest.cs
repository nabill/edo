using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public class SavedCardPaymentRequest : PaymentRequestBase
    {
        [JsonConstructor]
        public SavedCardPaymentRequest(decimal amount, Currencies currency, string securityCode, int cardId):
            base(amount, currency, securityCode)
        {
            CardId = cardId;
        }

        public int CardId { get; }
    }
}