using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct SavedCreditCardBookingPaymentRequest
    {
        [JsonConstructor]
        public SavedCreditCardBookingPaymentRequest(string referenceCode, string securityCode)
        {
            ReferenceCode = referenceCode;
            SecurityCode = securityCode;
        }
        
        public string ReferenceCode { get; }
        public string SecurityCode { get; }
    }
}