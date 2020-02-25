using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Locations
{
    public class LocationEqualityComparer : IEqualityComparer<Location>
    {
        public bool Equals(Location first, Location second)
        {
            if (first == null && second == null)
                return true;

            if (first == null || second == null)
                return false;

            return first.DefaultName == second.DefaultName
                && first.DefaultLocality == second.DefaultLocality
                && first.DefaultCountry == second.DefaultCountry
                && ((!first.Coordinates.IsValid && !second.Coordinates.IsValid) ||
                    first.Coordinates.Distance(second.Coordinates) < CoordinatesEqualityMaxDistance)
                && first.Source == second.Source
                && first.Type == second.Type;
        }


        public int GetHashCode(Location location)
        {
            return (location.DefaultName, location.DefaultCountry,
                location.DefaultLocality,
                location.Coordinates, location.Source,
                location.Type).GetHashCode();
        }


        private const int CoordinatesEqualityMaxDistance = 100;
    }
}