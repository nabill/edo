using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.CreditCard
{
    public readonly struct SaveCreditCardRequest
    {
        [JsonConstructor]
        public SaveCreditCardRequest(string number, string expirationDate, string holderName, string token, string referenceCode, CreditCardOwnerType ownerType)
        {
            Number = number;
            ExpirationDate = expirationDate;
            HolderName = holderName;
            Token = token;
            ReferenceCode = referenceCode;
            OwnerType = ownerType;
        }

        public string Number { get; }
        public string ExpirationDate { get; }
        public string HolderName { get; }
        public string Token { get; }
        public string ReferenceCode { get; }
        public CreditCardOwnerType OwnerType { get; }
    }
}
