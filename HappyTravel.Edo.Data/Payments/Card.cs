using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Data.Payments
{
    public class Card
    {
        public virtual int Id { get; set; }
        public virtual string CardNumber { get; set; }
        public virtual string ExpiryDate { get; set; }
        public virtual string Token { get; set; }
        public virtual string CardHolderName { get; set; }
    }
}
