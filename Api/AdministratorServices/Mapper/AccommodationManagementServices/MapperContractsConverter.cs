using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Geography;

namespace HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;

public static class MapperContractsConverter
{
    public static DetailedAccommodation Convert(
        HappyTravel.MapperContracts.Public.Management.Accommodations.DetailedAccommodations.DetailedAccommodation detailedAccommodation)
        => new()
        {
            HtId = detailedAccommodation.HtId,
            IsActive = detailedAccommodation.IsActive,
            SuppliersPriorities = detailedAccommodation.SuppliersPriorities,
            SuppliersRawAccommodationData = Convert(detailedAccommodation.SuppliersRawAccommodationData),
            Data = Convert(detailedAccommodation.Data),
            ManualCorrectedData = Convert(detailedAccommodation.ManualCorrectedData)
        };


    public static AccommodationData Convert(
        HappyTravel.MapperContracts.Public.Management.Accommodations.DetailedAccommodations.AccommodationData? accommodationData)
        => accommodationData is null
            ? new AccommodationData()
            : new AccommodationData
            {
                Name = accommodationData.Name,
                Category = accommodationData.Category,
                Contacts = accommodationData.Contacts,
                Location = Convert(accommodationData.Location)
            };


    public static DetailedLocationInfo Convert(
        HappyTravel.MapperContracts.Public.Management.Accommodations.DetailedAccommodations.DetailedLocationInfo detailedLocationInfo)
        => new()
        {
            Address = detailedLocationInfo.Address,
            CountryCode = detailedLocationInfo.CountryCode,
            IsHistoricalBuilding = detailedLocationInfo.IsHistoricalBuilding,
            Coordinates = new GeoPoint(detailedLocationInfo.Coordinates.Longitude, detailedLocationInfo.Coordinates.Latitude),
            CountryName = detailedLocationInfo.CountryName,
            LocalityName = detailedLocationInfo.LocalityName,
            LocalityZoneName = detailedLocationInfo.LocalityZoneName,
            LocationDescriptionCode = Convert(detailedLocationInfo.LocationDescriptionCode)
        };


    private static LocationDescriptionCodes Convert(HappyTravel.MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes descriptionCode)
        => descriptionCode switch
        {
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Airport => LocationDescriptionCodes.Airport,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Boutique => LocationDescriptionCodes.Boutique,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.City => LocationDescriptionCodes.City,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Desert => LocationDescriptionCodes.Desert,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Island => LocationDescriptionCodes.Island,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Mountains => LocationDescriptionCodes.Mountains,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Peripherals => LocationDescriptionCodes.Peripherals,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Port => LocationDescriptionCodes.Port,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Ranch => LocationDescriptionCodes.Ranch,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.CityCenter => LocationDescriptionCodes.CityCenter,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.OceanFront => LocationDescriptionCodes.OceanFront,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.OpenCountry => LocationDescriptionCodes.OpenCountry,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.RailwayStation => LocationDescriptionCodes.RailwayStation,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.WaterFront => LocationDescriptionCodes.WaterFront,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.SeaOrBeach => LocationDescriptionCodes.SeaOrBeach,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.CloseToCityCentre => LocationDescriptionCodes.CloseToCityCentre,
            MapperContracts.Public.Accommodations.Enums.LocationDescriptionCodes.Unspecified => LocationDescriptionCodes.Unspecified,
            _ => throw new NotSupportedException()
        };


    private static Dictionary<string, SupplierAccommodation> Convert(
        Dictionary<string, HappyTravel.MapperContracts.Public.Management.Accommodations.DetailedAccommodations.SupplierAccommodation>
            supplierAccommodationDictionary)
        => supplierAccommodationDictionary.ToDictionary(i => i.Key, i => Convert(i.Value));


    private static SupplierAccommodation Convert(
        HappyTravel.MapperContracts.Public.Management.Accommodations.DetailedAccommodations.SupplierAccommodation supplierAccommodation)
        => new()
        {
            SupplierCode = supplierAccommodation.SupplierCode,
            Data = Convert(supplierAccommodation.Data)
        };
}