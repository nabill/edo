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
        
        
        /// <summary>
        ///     Date the policy applies. A newer policy overwrites an older one.
        /// </summary>
        public DateTime FromDate { get; }

        /// <summary>
        ///     Percentage of the policy
        /// </summary>
        public double Percentage { get; }
    }
}