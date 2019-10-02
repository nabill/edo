using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct CreditCardPaymentResult
    {
        [JsonConstructor]
        public CreditCardPaymentResult(string secure3d, string referenceCode, string authorizationCode, string externalCode, string expiryDate, string cardNumber,
            PaymentStatuses status)
        {
            Secure3d = secure3d;
            ReferenceCode = referenceCode;
            AuthorizationCode = authorizationCode;
            ExternalCode = externalCode;
            ExpirationDate = expiryDate;
            CardNumber = cardNumber;
            Status = status;
        }

        public string Secure3d { get; }
        public string ReferenceCode { get; }
        public string AuthorizationCode { get; }
        public string ExternalCode { get; }
        public string ExpirationDate { get; }
        public string CardNumber { get; }
        public PaymentStatuses Status { get; }
    }
}
