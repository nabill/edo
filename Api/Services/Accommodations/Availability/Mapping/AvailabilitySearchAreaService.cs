using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Data;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AvailabilitySearchAreaService : IAvailabilitySearchAreaService
    {
        public AvailabilitySearchAreaService(IAccommodationMapperClient client, EdoContext edoContext)
        {
            _client = client;
            _edoContext = edoContext;
        }


        public async Task<Result<SearchArea>> GetSearchArea(List<string> htIds, string languageCode)
        {
            var (_, isFailure, mappings, error) = await _client.GetMappings(htIds, languageCode);
            if (isFailure)
                return Result.Failure<SearchArea>(error.Detail);

            if (!mappings.Any())
                return Result.Failure<SearchArea>("Could not find requested search locations");

            var locations = new List<Location>();
            var codes = new Dictionary<string, List<SupplierCodeMapping>>();

            foreach (var mapping in mappings)
            {
                locations.Add(mapping.Location);

                foreach (var accommodationMapping in mapping.AccommodationMappings)
                    FillSupplierCodes(accommodationMapping, codes, mapping.Location);
            }

            return new SearchArea
            {
                Locations = locations,
                AccommodationCodes = codes
            };


            void FillSupplierCodes(in AccommodationMapping accommodationMapping, Dictionary<string, List<SupplierCodeMapping>> dictionary, Location location)
            {
                foreach (var supplierInfo in accommodationMapping.SupplierCodes)
                {
                    var supplierCodeMapping = new SupplierCodeMapping
                    {
                        AccommodationHtId = accommodationMapping.HtId,
                        SupplierCode = supplierInfo.Value,
                        CountryHtId = location.CountryHtId,
                        LocalityHtId = location.LocalityHtId,
                        MarketId = _edoContext.Countries
                            .Where(c => c.Code == location.CountryCode)
                            .Select(c => c.MarketId)
                            .FirstOrDefault()
                    };

                    var supplierCode = supplierInfo.Key;

                    if (dictionary.TryGetValue(supplierCode, out var supplierCodeMappings))
                        supplierCodeMappings.Add(supplierCodeMapping);
                    else
                        dictionary[supplierCode] = new List<SupplierCodeMapping> { supplierCodeMapping };
                }
            }
        }


        private readonly IAccommodationMapperClient _client;
        private readonly EdoContext _edoContext;
    }
}