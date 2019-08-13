using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HotelFeatureTypes
    {
        None,
        Honeymooners,
        AdjoiningRooms,
        CommunicatingRooms,
        HighestFloorRoom,
        LowerFloorRoom,
        EarlyCheckIn,
        LateArrival,
        KingSizeBed,
        QueenSizeBed,
        DoubleBedForSoleUse,
        NoSmokingRoom,
        AirConditioner,
        ChampagneBottle,
        FruitBasket,
        WineBottle
    }
}
