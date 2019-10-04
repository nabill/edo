using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct DeadlineDetails
    {
        [JsonConstructor]
        public DeadlineDetails(int availabilityId, string tariffCode, DateTime date, List<CancellationPolicy> policies, List<string> remarkCodes)
        {
            AvailabilityId = availabilityId;
            Date = date;
            Policies = policies ?? new List<CancellationPolicy>(0);
            RemarkCodes = remarkCodes ?? new List<string>(0);
            TariffCode = tariffCode;
        }


        public int AvailabilityId { get; }
        public DateTime Date { get; }
        public List<CancellationPolicy> Policies { get; }
        public List<string> RemarkCodes { get; }
        public string TariffCode { get; }
    }
}

