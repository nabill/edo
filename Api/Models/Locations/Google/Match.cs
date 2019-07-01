using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Locations.Google
{
    public readonly struct Match
    {
        [JsonConstructor]
        public Match(int length, int offset)
        {
            Length = length;
            Offset = offset;
        }


        [JsonProperty("length")]
        public int Length { get; }

        [JsonProperty("offset")]
        public int Offset { get; }
    }
}
