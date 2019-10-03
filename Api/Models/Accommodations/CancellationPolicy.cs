using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct CancellationPolicy
    {
        public CancellationPolicy(DateTime fromDate, double percentage)
        {
            FromDate = fromDate;
            Percentage = percentage;
        }
        
        public DateTime FromDate { get; }
        public double Percentage { get; }
    }
}
