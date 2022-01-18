using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Payments
{
    public class AgencyAccount : IEntity
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public decimal Balance { get; set; }
        public Currencies Currency { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool IsActive { get; set; }
    }
}