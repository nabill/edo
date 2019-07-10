using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct AvailabilityResponse
    {
        [JsonConstructor]
        private AvailabilityResponse(int availabilityId, int numberOfNights, DateTime checkInDate, DateTime checkOutDate, List<SlimAvailabilityResult> results, string source = "Netstorming")
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfNights = numberOfNights;
            Results = results;
            Source = source;
        }


        public int AvailabilityId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public int NumberOfNights { get; }
        public List<SlimAvailabilityResult> Results { get; }
        public string Source { get; }
    }
}
