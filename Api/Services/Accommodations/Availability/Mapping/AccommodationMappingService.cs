using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AccommodationMappingService : IAccommodationMappingService
    {
        public AccommodationMappingService(IAccommodationMapperClient client, IDoubleFlow flow)
        {
            _client = client;
            _flow = flow;
        }


        public async Task<Result<LocationDescriptor>> GetLocationDescriptor(string htId, AccommodationMapperLocationType type)
        {
            var key = _flow.BuildKey(nameof(AccommodationMappingService), nameof(GetLocationDescriptor),
                type.ToString(), htId);
            
            var descriptor = await _flow.GetOrSetAsync(key,
                () => GetFromMapperService(htId),
                LocationDescriptorCacheLifeTime);

            return descriptor ?? Result.Failure<LocationDescriptor>("Could not get location descriptor");


            async Task<LocationDescriptor?> GetFromMapperService(string htId)
            {
                var (_, isFailure, mapping, _) = await _client.GetMapping(htId, type);
                if (isFailure)
                    return default;

                var codes = new Dictionary<Suppliers, List<SupplierCodeMapping>>();
                foreach (var accommodationMapping in mapping.AccommodationMappings)
                {
                    foreach (var supplierCodes in accommodationMapping.SupplierCodes)
                    {
                        var codeMappings = supplierCodes.Value
                            .Select(sc => new SupplierCodeMapping
                            {
                                HtId = accommodationMapping.HtId,
                                SupplierCode = sc
                            })
                            .ToList();
                        
                        if (codes.TryGetValue(supplierCodes.Key, out var supplierCodeMappings))
                            supplierCodeMappings.AddRange(codeMappings);
                        else
                            codes[supplierCodes.Key] = codeMappings;
                    }
                }

                return new LocationDescriptor
                {
                    Location = mapping.Location,
                    AccommodationCodes = codes
                };
            }
        }


        private static readonly TimeSpan LocationDescriptorCacheLifeTime = TimeSpan.FromMinutes(10);

        private readonly IAccommodationMapperClient _client;
        private readonly IDoubleFlow _flow;
    }
}