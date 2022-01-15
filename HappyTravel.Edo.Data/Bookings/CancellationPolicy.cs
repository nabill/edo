using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Data.Bookings
{
    public class CancellationPolicy
    {
        // EF constructor
        private CancellationPolicy() { }

        [JsonConstructor]
        public CancellationPolicy(DateTimeOffset fromDate, double percentage)
        {
            FromDate = fromDate;
            Percentage = percentage;
        }
        
        public DateTimeOffset FromDate { get; set; }
        public double Percentage { get; set; }
    }
}