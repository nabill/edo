using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct DeadlineDetails
    {
        public DeadlineDetails(int availabilityId, string tariffCode, DateTime date, List<CancellationPolicy> policies, List<string> remarkCodes)
        {
            AvailabilityId = availabilityId;
            Date = date;
            Policies = policies ?? new List<CancellationPolicy>();
            RemarkCodes = remarkCodes ?? new List<string>();
            TariffCode = tariffCode;
        }


        public int AvailabilityId { get; }
        public DateTime Date { get; }
        public List<CancellationPolicy> Policies { get; }
        public List<string> RemarkCodes { get; }
        public string TariffCode { get; }
    }
}

