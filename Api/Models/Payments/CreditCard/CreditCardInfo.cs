using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Payments.CreditCard
{
    public class CreditCardInfo
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string ExpirationDate { get; set; }
        public string HolderName { get; set; }
        public CreditCardOwnerType OwnerType { get; set; }
    }
}
