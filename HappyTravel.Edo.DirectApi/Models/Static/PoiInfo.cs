using System.Text.Json.Serialization;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct PoiInfo
    {
        [JsonConstructor]
        public PoiInfo(string name, double distance, double time, PoiTypes type, string? description = null)
        {
            Name = name;
            Description = description ?? string.Empty;
            Distance = distance;
            Time = time;
            Type = type;
        }


        /// <summary>
        ///     Name of the point of interest
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Description of the point of interest
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Distance to the point of interest in meters
        /// </summary>
        public double Distance { get; }

        /// <summary>
        ///     Travel time to the point of interest in seconds
        /// </summary>
        public double Time { get; }

        /// <summary>
        ///     Type of point of interest
        /// </summary>
        public PoiTypes Type { get; }
    }
}