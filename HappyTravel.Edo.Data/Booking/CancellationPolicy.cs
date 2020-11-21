using System;

namespace HappyTravel.Edo.Data.Booking
{
    public class CancellationPolicy
    {
        // EF constructor
        private CancellationPolicy() { }

        public CancellationPolicy(DateTime fromDate, double percentage)
        {
            FromDate = fromDate;
            Percentage = percentage;
        }
        
        public DateTime FromDate { get; set; }
        public double Percentage { get; set; }
    }
}