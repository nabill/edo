using System;

namespace HappyTravel.Edo.Data.Customers
{
    public class Branch
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public string Title { get; set; }
        public bool IsDefault { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}