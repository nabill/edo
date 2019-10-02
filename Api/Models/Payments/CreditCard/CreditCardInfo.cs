using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.CreditCard
{
    public readonly struct CreditCardInfo
    {
        [JsonConstructor]
        public CreditCardInfo(int id, string number, string expirationDate, string holderName, CreditCardOwnerType ownerType, string token)
        {
            Id = id;
            Number = number;
            ExpirationDate = expirationDate;
            HolderName = holderName;
            OwnerType = ownerType;
            Token = token;
        }

        public int Id { get; }
        public string Number { get; }
        public string ExpirationDate { get; }
        public string HolderName { get; }
        public CreditCardOwnerType OwnerType { get; }
        public string Token { get; }
    }
}
