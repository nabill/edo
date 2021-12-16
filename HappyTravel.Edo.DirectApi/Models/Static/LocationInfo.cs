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
        ///     The location address.
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     Location coordinates.
        /// </summary>
        public GeoPoint Coordinates { get; }

        /// <summary>
        ///     The country code in the ISO 3166-1 Alpha-2 format.
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// Happy Travel country Id
        /// </summary>
        public string CountryId { get; }

        /// <summary>
        ///     The location country name.
        /// </summary>
        public string Country { get; }

        /// <summary>
        ///     Indicates if a location a historical place.
        /// </summary>
        public bool IsHistoricalBuilding { get; }

        /// <summary>
        /// Happy Travel localityId
        /// </summary>
        public string LocalityId { get; }

        /// <summary>
        ///     The locality name.
        /// </summary>
        public string Locality { get; }

        /// <summary>
        /// Happy Travel locality zone Id
        /// </summary>
        public string? LocalityZoneId { get; }

        /// <summary>
        ///     The locality zone name.
        /// </summary>
        public string? LocalityZone { get; }

        /// <summary>
        ///     The description of a location.
        /// </summary>
        public LocationDescriptionCodes LocationDescriptionCode { get; }

        /// <summary>
        ///     The list of transportation facility or POI.
        /// </summary>
        public List<PoiInfo> PointsOfInterests { get; }
    }
}