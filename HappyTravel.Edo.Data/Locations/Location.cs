using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.GeoData.Enums;
using NetTopologySuite.Geometries;

namespace HappyTravel.Edo.Data.Locations
{
    public class Location
    {
        public Guid Id { get; set; }
        public string Country { get; set; }
        public Point Coordinates { get; set; } = new Point(0, 0);
        public int DistanceInMeters { get; set; }
        public string Locality { get; set; }
        public string Name { get; set; }
        public PredictionSources Source { get; set; }
        public LocationTypes Type { get; set; }
        public List<DataProviders> DataProviders { get; set; }
        public DateTime Modified { get; set; }

        public string DefaultName { get; set; }
        public string DefaultCountry { get; set; }
        public string DefaultLocality { get; set; }


        public override bool Equals(object obj)
        {
            if (!(obj is Location))
                return false;

            var otherLocation = (Location) obj;

            return DefaultName == otherLocation.DefaultName
                && DefaultLocality == otherLocation.DefaultLocality
                && DefaultCountry == otherLocation.DefaultCountry
                && ((!Coordinates.IsValid && !otherLocation.Coordinates.IsValid) || Coordinates.Distance(otherLocation.Coordinates) < 100)
                && Source == otherLocation.Source
                && Type == otherLocation.Type;
        }


        public override int GetHashCode() => (DefaultName, DefaultLocality, DefaultCountry, Coordinates, Source, Type).GetHashCode();
    }
}