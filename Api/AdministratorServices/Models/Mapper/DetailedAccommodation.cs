using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct DetailedAccommodation
    {
        public DetailedAccommodation(AccommodationData data, Dictionary<Suppliers, AccommodationData> suppliersRawAccommodationData, AccommodationData manualCorrectedData, Dictionary<AccommodationDataTypes, List<Suppliers>> suppliersPriorities)
        {
            Data = data;
            SuppliersRawAccommodationData = suppliersRawAccommodationData ?? new ();
            ManualCorrectedData = manualCorrectedData;
            SuppliersPriorities = suppliersPriorities ?? new ();
        }


        public AccommodationData Data { get; init; }
        public Dictionary<Suppliers, AccommodationData> SuppliersRawAccommodationData { get; init; }
        public AccommodationData ManualCorrectedData { get; init; }
        public Dictionary<AccommodationDataTypes, List<Suppliers>> SuppliersPriorities { get; init; }
    }
}