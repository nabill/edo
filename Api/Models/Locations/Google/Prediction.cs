using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Locations.Google
{
    public readonly struct Prediction
    {
        [JsonConstructor]
        public Prediction(string id, string description, List<Match> matches, List<string> types)
        {
            Id = id;
            Description = description;
            Matches = matches;
            Types = types;
        }


        [JsonProperty("place_id")]
        public string Id { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("matched_substrings")]
        public List<Match> Matches { get; }

        [JsonProperty("types")]
        public List<string> Types { get; }
    }
}
