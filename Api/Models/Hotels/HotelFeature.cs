using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct HotelFeature
    {
        [JsonConstructor]
        public HotelFeature(HotelFeatureTypes type, string value)
        {
            Type = type;
            Value = value;
        }


        public HotelFeatureTypes Type { get; }
        public string Value { get; }
    }
}
