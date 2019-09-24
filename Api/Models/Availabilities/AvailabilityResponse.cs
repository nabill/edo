using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct AvailabilityResponse
    {
        [JsonConstructor]
        private AvailabilityResponse(int availabilityId, int numberOfNights, DateTime checkInDate, DateTime checkOutDate, List<SlimAvailabilityResult> results)
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfNights = numberOfNights;
            Results = results;
        }
        
        public AvailabilityResponse(AvailabilityResponse availabilityResponse, List<SlimAvailabilityResult> results)
        {
            AvailabilityId = availabilityResponse.AvailabilityId;
            CheckInDate = availabilityResponse.CheckInDate;
            CheckOutDate = availabilityResponse.CheckOutDate;
            NumberOfNights = availabilityResponse.NumberOfNights;
            Results = results;
        }
        
        public int AvailabilityId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public int NumberOfNights { get; }
        public List<SlimAvailabilityResult> Results { get; }
        
        public override bool Equals(object obj) => obj is AvailabilityResponse other && Equals(other);

        public bool Equals(AvailabilityResponse other)
        {
            return (AvailabilityId, CheckInDate, NumberOfNights, Results) ==
                   (other.AvailabilityId, other.CheckInDate, other.NumberOfNights, other.Results);
        }

        public override int GetHashCode() => (AvailabilityId, CheckInDate, NumberOfNights, Results).GetHashCode();
    }
}
