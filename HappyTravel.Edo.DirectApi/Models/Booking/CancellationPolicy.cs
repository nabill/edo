using System;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct CancellationPolicy
    {
        [JsonConstructor]
        public CancellationPolicy(DateTime fromDate, double percentage)
        {
            FromDate = fromDate;
            Percentage = percentage;
        }
        
        
        public DateTime FromDate { get; }
        public double Percentage { get; }
    }
}