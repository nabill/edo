using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct Deadline
    {
        [JsonConstructor]
        public Deadline(DateTime? date, List<CancellationPolicy>? policies, List<string>? remarks, bool isFinal)
        {
            Date = date;
            IsFinal = isFinal;
            Policies = policies ?? new List<CancellationPolicy>();
            Remarks = remarks ?? new List<string>(0);
        }
        
        
        public DateTime? Date { get; }
        public bool IsFinal { get; }
        public List<CancellationPolicy> Policies { get; }
        public List<string> Remarks { get; }
    }
}