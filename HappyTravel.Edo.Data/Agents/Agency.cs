using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Agents
{
    public class Agency
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int? ParentId { get; set; }
        
        public bool IsActive { get; set; }
        public List<int> Ancestors { get; init; }
    }
}