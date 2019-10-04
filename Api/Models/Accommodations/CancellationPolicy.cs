using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
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
