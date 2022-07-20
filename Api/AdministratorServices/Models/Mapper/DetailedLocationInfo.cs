using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.Geography;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;

public class DetailedLocationInfo
{
    public string Address { get; init; } = string.Empty;
    public GeoPoint Coordinates { get; init; }
    public string CountryCode { get; init; } = string.Empty;
    public string CountryName { get; init; } = string.Empty;
    public bool IsHistoricalBuilding { get; init; }
    public string? LocalityName { get; init; }
    public string? LocalityZoneName { get; init; }
    public LocationDescriptionCodes LocationDescriptionCode { get; init; }
    public List<PoiInfo> PointsOfInterests { get; init; } = new();
}