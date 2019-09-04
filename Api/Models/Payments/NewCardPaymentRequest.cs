using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public class NewCardPaymentRequest: PaymentRequestBase
    {
        [JsonConstructor]
        public NewCardPaymentRequest(decimal amount, Currencies currency, string securityCode, string number, string expiryDate, string holderName, bool rememberMe) :
            base(amount, currency, securityCode)
        {
            Number = number;
            ExpiryDate = expiryDate;
            HolderName = holderName;
            RememberMe = rememberMe;
        }

        public string Number { get; }
        public string ExpiryDate { get; }
        public string HolderName { get; }
        public bool RememberMe { get; }
    }
}