using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Booking
{
    public class Deadline
    {
        // EF constructor
        private Deadline() {}

        public Deadline(DateTime? date, List<CancellationPolicy> policies, List<string> remarks = null)
        {
            Date = date;
            Policies = policies;
            Remarks = remarks ?? new List<string>(0);
        }
        
        public DateTime? Date { get; set; }
        public List<CancellationPolicy> Policies { get; set; }
        public List<string> Remarks { get; set; }
    }
}