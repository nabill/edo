using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct HotelFeatureInfo
    {
        [JsonConstructor]
        public HotelFeatureInfo(HotelFeatureTypes type, bool isValueRequired)
        {
            Type = type;
            IsValueRequired = isValueRequired;
        }


        public HotelFeatureTypes Type { get; }
        public bool IsValueRequired { get; }
    }
}
