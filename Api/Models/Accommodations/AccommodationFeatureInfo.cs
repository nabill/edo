using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AccommodationFeatureInfo
    {
        [JsonConstructor]
        public AccommodationFeatureInfo(AccommodationFeatureTypes type, bool isValueRequired)
        {
            Type = type;
            IsValueRequired = isValueRequired;
        }


        public AccommodationFeatureTypes Type { get; }
        public bool IsValueRequired { get; }
    }
}
