using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using GeoPoint = HappyTravel.Geography.GeoPoint;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public record MultilingualLocationDetails
{
    public string? CountryCode { get; init; }
    public MultiLanguage.MultiLanguage<string>? Country { get; init;}
    public GeoPoint Coordinates { get; init;}
    public MultiLanguage.MultiLanguage<string>? Address { get; init;}
    public bool IsHistoricalBuilding { get; init;}
    public string? SupplierLocalityCode { get; init;}
    public MultiLanguage.MultiLanguage<string>? Locality { get; init;}
    public string? SupplierLocalityZoneCode { get; init;}
    public MultiLanguage.MultiLanguage<string>? LocalityZone { get; init;}
    public LocationDescriptionCodes LocationDescriptionCode { get; init;}
    public List<PoiDetails> PointsOfInterests { get; init; } = new();
}