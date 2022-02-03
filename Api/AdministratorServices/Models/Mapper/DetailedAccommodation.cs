using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct DetailedAccommodation
    {
        public DetailedAccommodation(AccommodationData data, Dictionary<int, AccommodationData> suppliersRawAccommodationData, AccommodationData manualCorrectedData, Dictionary<AccommodationDataTypes, List<int>> suppliersPriorities)
        {
            Data = data;
            SuppliersRawAccommodationData = suppliersRawAccommodationData ?? new ();
            ManualCorrectedData = manualCorrectedData;
            SuppliersPriorities = suppliersPriorities ?? new ();
        }


        public AccommodationData Data { get; init; }
        public Dictionary<int, AccommodationData> SuppliersRawAccommodationData { get; init; }
        public AccommodationData ManualCorrectedData { get; init; }
        public Dictionary<AccommodationDataTypes, List<int>> SuppliersPriorities { get; init; }
    }
}