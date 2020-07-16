using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class CounterpartyAccount : IEntity
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public decimal Balance { get; set; }
        public Currencies Currency { get; set; }
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
    }
}