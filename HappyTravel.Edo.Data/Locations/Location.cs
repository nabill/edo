using System;
using GeoAPI.Geometries;
using HappyTravel.EdoContracts.GeoData.Enums;
using NetTopologySuite.Geometries;

namespace HappyTravel.Edo.Data.Locations
{
    public class Location
    {
        public Guid Id { get; set; }
        public string Country { get; set; }
        public IPoint Coordinates { get; set; } = new Point(0, 0);
        public int DistanceInMeters { get; set; }
        public string Locality { get; set; }
        public string Name { get; set; }
        public PredictionSources Source { get; set; }
        public LocationTypes Type { get; set; }
    }
}
