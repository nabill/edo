using HappyTravel.Edo.Api.Models.Accommodations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct AccommodationFeature
    {
        [JsonConstructor]
        public AccommodationFeature(AccommodationFeatureTypes type, string value)
        {
            Type = type;
            Value = value;
        }


        public AccommodationFeatureTypes Type { get; }
        public string Value { get; }
    }
}