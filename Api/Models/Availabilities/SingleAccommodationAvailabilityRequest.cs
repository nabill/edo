using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct SingleAccommodationAvailabilityRequest
    {
        [JsonConstructor]
        public SingleAccommodationAvailabilityRequest(long availabilityId)
        {
            AvailabilityId = availabilityId;
        }


        public long AvailabilityId { get; }
    }
}