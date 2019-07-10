using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct DirectionInfo
    {
        [JsonConstructor]
        public DirectionInfo(string name, double distanceInKilometers, double timeToInMinutes, DirectionTypes type)
        {
            Name = name;
            DistanceInKilometers = distanceInKilometers;
            TimeToInMinutes = timeToInMinutes;
            Type = type;
        }


        public string Name { get; }

        public double DistanceInKilometers { get; }
        
        public double TimeToInMinutes { get; }

        public DirectionTypes Type { get; }
    }
}
