using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Locations.Google;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Prediction
    {
        public Prediction(string id, PredictionSources source, List<Match> matches, LocationTypes type, string value)
        {
            Id = id;
            Matches = matches;
            Source = source;
            Type = type;
            Value = value;
        }


        public string Id { get; }
        public List<Match> Matches { get; }
        public PredictionSources Source { get; }
        public LocationTypes Type { get; }
        public string Value { get; }
    }
}
