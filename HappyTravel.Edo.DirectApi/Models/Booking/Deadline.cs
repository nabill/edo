using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct Deadline
    {
        [JsonConstructor]
        public Deadline(DateTime? date, List<CancellationPolicy>? policies, List<string>? remarks)
        {
            Date = date;
            Policies = policies ?? new List<CancellationPolicy>();
            Remarks = remarks ?? new List<string>(0);
        }
        
        /// <summary>
        ///     Free cancellation is available before this date
        /// </summary>
        public DateTime? Date { get; }

        /// <summary>
        ///     List of cancellation policies
        /// </summary>
        public List<CancellationPolicy> Policies { get; }

        /// <summary>
        ///     Extra notes on the deadline
        /// </summary>
        public List<string> Remarks { get; }
    }
}