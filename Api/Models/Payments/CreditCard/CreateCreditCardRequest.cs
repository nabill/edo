using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.CreditCard
{
    public readonly struct CreateCreditCardRequest
    {
        [JsonConstructor]
        public CreateCreditCardRequest (string number, string expirationDate, string holderName, string securityCode, CreditCardOwnerType ownerType)
        {
            Number = number;
            ExpirationDate = expirationDate;
            HolderName = holderName;
            SecurityCode = securityCode;
            OwnerType = ownerType;
        }

        public string Number { get; }
        public string ExpirationDate { get; }
        public string HolderName { get; }
        public string SecurityCode { get; }
        public CreditCardOwnerType OwnerType { get; }
    }
}
