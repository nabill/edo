using System;

namespace HappyTravel.Edo.Data.Agents
{
    public class Agency
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}