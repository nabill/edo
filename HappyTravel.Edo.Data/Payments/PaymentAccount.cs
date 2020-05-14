using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class PaymentAccount : IEntity
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public decimal Balance { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal AuthorizedBalance { get; set; }
        public Currencies Currency { get; set; }
        public DateTime Created { get; set; }
    }
}