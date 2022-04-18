using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.AdministratorServices.Locations;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AvailabilitySearchAreaService : IAvailabilitySearchAreaService
    {
        public AvailabilitySearchAreaService(IAccommodationMapperClient client, IMarketManagementStorage marketStorage)
        {
            _client = client;
            _marketStorage = marketStorage;
        }


        public async Task<Result<SearchArea>> GetSearchArea(List<string> htIds, string languageCode, CancellationToken cancellationToken)
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
                var market = await _marketStorage.Get(mapping.Location.CountryCode, cancellationToken);

                foreach (var accommodationMapping in mapping.AccommodationMappings)
                    FillSupplierCodes(accommodationMapping, codes, mapping.Location, market?.Id ?? -1);
            }

            return new SearchArea
            {
                Locations = locations,
                AccommodationCodes = codes
            };


            void FillSupplierCodes(in AccommodationMapping accommodationMapping, Dictionary<string, List<SupplierCodeMapping>> dictionary, Location location, int marketId)
            {
                foreach (var supplierInfo in accommodationMapping.SupplierCodes)
                {
                    var supplierCodeMapping = new SupplierCodeMapping
                    {
                        AccommodationHtId = accommodationMapping.HtId,
                        SupplierCode = supplierInfo.Value,
                        CountryHtId = location.CountryHtId,
                        LocalityHtId = location.LocalityHtId,
                        MarketId = marketId
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
        private readonly IMarketManagementStorage _marketStorage;
    }
}