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


        /// <summary>
        /// The length of a query occurrence.
        /// </summary>
        [JsonProperty("length")]
        public int Length { get; }

        /// <summary>
        /// The position of a query occurrence.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; }
    }
}
