using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Locations.Google;

namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Prediction
    {
        public Prediction(string id, List<Match> matches, LocationTypes type, string value)
        {
            Id = id;
            Matches = matches;
            Type = type;
            Value = value;
        }


        public string Id { get; }
        public List<Match> Matches { get; }
        public LocationTypes Type { get; }
        public string Value { get; }
    }
}
