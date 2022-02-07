using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using GeoPoint = HappyTravel.Geography.GeoPoint;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public struct MultilingualLocationDetails
{
    public MultilingualLocationDetails(
        string countryCode,
        MultiLanguage.MultiLanguage<string> country,
        in GeoPoint coordinates,
        MultiLanguage.MultiLanguage<string> address,
        LocationDescriptionCodes locationDescriptionCode,
        List<PoiDetails> pointsOfInterests,
        string supplierLocalityCode = null,
        MultiLanguage.MultiLanguage<string> locality = null,
        string supplierLocalityZoneCode = null,
        MultiLanguage.MultiLanguage<string> localityZone = null,
        bool isHistoricalBuilding = false)
    {
        CountryCode = countryCode;
        Country = country;
        Locality = locality;
        LocalityZone = localityZone;
        Address = address;
        Coordinates = coordinates;
        LocationDescriptionCode = locationDescriptionCode;
        PointsOfInterests = pointsOfInterests ?? new List<PoiDetails>(0);
        IsHistoricalBuilding = isHistoricalBuilding;
        SupplierLocalityCode = supplierLocalityCode;
        SupplierLocalityZoneCode = supplierLocalityZoneCode;
    }

    
    public MultiLanguage.MultiLanguage<string> Address { get; }
    public GeoPoint Coordinates { get; }
    public string CountryCode { get; }
    public MultiLanguage.MultiLanguage<string> Country { get; }
    public bool IsHistoricalBuilding { get; }
    public string SupplierLocalityCode { get; }
    public MultiLanguage.MultiLanguage<string> Locality { get; }
    public string SupplierLocalityZoneCode { get; }
    public MultiLanguage.MultiLanguage<string> LocalityZone { get; }
    public LocationDescriptionCodes LocationDescriptionCode { get; }
    public List<PoiDetails> PointsOfInterests { get; }
}