using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AvailabilitySearchAreaService : IAvailabilitySearchAreaService
    {
        public AvailabilitySearchAreaService(IAccommodationMapperClient client)
        {
            _client = client;
        }


        public async Task<Result<SearchArea>> GetSearchArea(List<string> htIds, string languageCode)
        {
            var (_, isFailure, mappings, error) = await _client.GetMappings(htIds, languageCode);
            if (isFailure)
                return Result.Failure<SearchArea>(error.Detail);
            
            if (!mappings.Any())
                return Result.Failure<SearchArea>("Could not find requested search locations");

            var locations = new List<Location>();
            var codes = new Dictionary<Suppliers, List<SupplierCodeMapping>>();
            
            foreach (var mapping in mappings)
            {
                locations.Add(mapping.Location);
                
                foreach (var accommodationMapping in mapping.AccommodationMappings)
                    FillSupplierCodes(accommodationMapping, codes);
            }

            return new SearchArea
            {
                Locations = locations,
                AccommodationCodes = codes
            };


            static void FillSupplierCodes(in AccommodationMapping accommodationMapping, Dictionary<Suppliers, List<SupplierCodeMapping>> dictionary)
            {
                foreach (var supplierCode in accommodationMapping.SupplierCodes)
                {
                    var supplierCodeMapping = new SupplierCodeMapping
                    {
                        HtId = accommodationMapping.HtId,
                        SupplierCode = supplierCode.Value
                    };

                    var supplier = (Suppliers) supplierCode.Key;

                    if (dictionary.TryGetValue(supplier, out var supplierCodeMappings))
                        supplierCodeMappings.Add(supplierCodeMapping);
                    else
                        dictionary[supplier] = new List<SupplierCodeMapping> {supplierCodeMapping};
                }
            }
        }


        private readonly IAccommodationMapperClient _client;
    }
}