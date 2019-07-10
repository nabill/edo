using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Locations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct SearchLocation
    {
        [JsonConstructor]
        public SearchLocation(PredictionResult predictionResult, GeoPoint coordinates, [Range(0, 40000)] int distance)
        {
            Coordinates = coordinates;
            Distance = distance;
            PredictionResult = predictionResult;
        }


        /// <summary>
        /// Central point of a search.
        /// </summary>
        public GeoPoint Coordinates { get; }
        
        /// <summary>
        /// Search distance.
        /// </summary>
        public int Distance { get; }

        /// <summary>
        /// The result of a prediction query.
        /// </summary>
        public PredictionResult PredictionResult { get; }
    }
}
