using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct DetailedAccommodation
    {
        public DetailedAccommodation(string htId, AccommodationData data, Dictionary<string, AccommodationData> suppliersRawAccommodationData,
            AccommodationData manualCorrectedData, Dictionary<AccommodationDataTypes, List<int>> suppliersPriorities, bool isActive)
        {
            Data = data;
            SuppliersRawAccommodationData = suppliersRawAccommodationData ?? new();
            ManualCorrectedData = manualCorrectedData;
            SuppliersPriorities = suppliersPriorities ?? new();
            HtId = htId;
            IsActive = isActive;
        }


        public string HtId { get; init; }
        public AccommodationData Data { get; init; }
        public Dictionary<string, AccommodationData> SuppliersRawAccommodationData { get; init; }
        public AccommodationData ManualCorrectedData { get; init; }
        public Dictionary<AccommodationDataTypes, List<int>> SuppliersPriorities { get; init; }
        public bool IsActive { get; init; }
    }
}