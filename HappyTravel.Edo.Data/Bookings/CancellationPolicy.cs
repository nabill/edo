using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Data.Bookings
{
    public class CancellationPolicy
    {
        // EF constructor
        private CancellationPolicy() { }

        [JsonConstructor]
        public CancellationPolicy(DateTime fromDate, double percentage, string remark)
        {
            FromDate = fromDate;
            Percentage = percentage;
            Remark = remark;
        }
        
        public DateTime FromDate { get; set; }
        public double Percentage { get; set; }
        public string Remark { get; set; }
    }
}