using System.Collections.Generic;
using System.Text.Json.Serialization;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct LocationInfo
    {
        [JsonConstructor]
        public LocationInfo(string countryCode, string countryId, string country, string? localityId, string? locality, string? localityZoneId,
            string? localityZone, in GeoPoint coordinates, string address, LocationDescriptionCodes locationDescriptionCode, List<PoiInfo>? pointsOfInterests,
            bool isHistoricalBuilding = false)
        {
            CountryCode = countryCode;
            CountryId = countryId;
            Country = country;
            // TODO check nullability
            Locality = locality;
            LocalityId = localityId;
            LocalityZoneId = localityZoneId;
            LocalityZone = localityZone;
            Address = address;
            Coordinates = coordinates;
            LocationDescriptionCode = locationDescriptionCode;
            PointsOfInterests = pointsOfInterests ?? new List<PoiInfo>(0);
            IsHistoricalBuilding = isHistoricalBuilding;
        }


        /// <summary>
        ///     Address of the location
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     Coordinates of the location
        /// </summary>
        public GeoPoint Coordinates { get; }

        /// <summary>
        ///     Two-letter country code in ISO 3166-1 Alpha-2 format
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        ///     Happytravel.com country ID
        /// </summary>
        public string CountryId { get; }

        /// <summary>
        ///     Name of the country
        /// </summary>
        public string Country { get; }

        // TODO: do we need this?
        /// <summary>
        ///     Indicates if the location is a historical place
        /// </summary>
        public bool IsHistoricalBuilding { get; }

        /// <summary>
        ///     Happytravel.com locality ID
        /// </summary>
        public string LocalityId { get; }

        /// <summary>
        ///     Name of the locality
        /// </summary>
        public string Locality { get; }

        /// <summary>
        ///     Happytravel.com locality zone ID
        /// </summary>
        public string? LocalityZoneId { get; }

        /// <summary>
        ///     Name of the locality zone
        /// </summary>
        public string? LocalityZone { get; }

        /// <summary>
        ///     Description of a location
        /// </summary>
        public LocationDescriptionCodes LocationDescriptionCode { get; }

        /// <summary>
        ///     List of transportation facilities or other points of interest
        /// </summary>
        public List<PoiInfo> PointsOfInterests { get; }
    }
}