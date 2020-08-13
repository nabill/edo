using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step1
{
    public class FirstStepAvailabilitySearchService : IFirstStepAvailabilitySearchService
    {
        public FirstStepAvailabilitySearchService(IAvailabilitySearchScheduler searchScheduler, 
            IAvailabilityStorage storage,
            IAccommodationDuplicatesService duplicatesService,
            IProviderRouter providerRouter,
            IMemoryFlow memoryFlow)
        {
            _searchScheduler = searchScheduler;
            _storage = storage;
            _duplicatesService = duplicatesService;
            _providerRouter = providerRouter;
            _memoryFlow = memoryFlow;
        }
        
        public Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode) => _searchScheduler.StartSearch(request, agent, languageCode);


        public async Task<AvailabilitySearchState> GetState(Guid searchId)
        {
            var providerSearchStates = await _storage.GetProviderResults<ProviderAvailabilitySearchState>(searchId);
            var searchStates = providerSearchStates
                .Where(s => !s.Result.Equals(default))
                .ToDictionary(s => s.DataProvider, s => s.Result);
            
            return AvailabilitySearchState.FromProviderStates(searchId, searchStates);
        }
        
        public async Task<IEnumerable<ProviderData<AvailabilityResult>>> GetResult(Guid searchId, AgentContext agent)
        {
            var accommodationDuplicates = await _duplicatesService.Get(agent);
            
            var key = _memoryFlow.BuildKey(nameof(AvailabilityStorage), searchId.ToString());
            if (!_memoryFlow.TryGetValue(key, out List<(DataProviders DataProvider, AvailabilityWithTimestamp Result)> providerSearchResults))
            {
                providerSearchResults = (await _storage.GetProviderResults<AvailabilityWithTimestamp>(searchId))
                    .Where(t => !t.Result.Details.Equals(default))
                    .ToList();

                if ((await GetState(searchId)).TaskState == AvailabilitySearchTaskState.Completed)
                    _memoryFlow.Set(key, providerSearchResults, CacheExpirationTime);
            }

            return CombineAvailabilities(providerSearchResults);


            IEnumerable<ProviderData<AvailabilityResult>> CombineAvailabilities(List<(DataProviders ProviderKey, AvailabilityWithTimestamp Availability)> availabilities)
            {
                if (availabilities == null || !availabilities.Any())
                    return Enumerable.Empty<ProviderData<AvailabilityResult>>();

                return availabilities
                    .OrderBy(r => r.Availability.TimeStamp)
                    .SelectMany(providerResults =>
                    {
                        var (providerKey, providerAvailability) = providerResults;
                        var details = providerAvailability.Details;
                        var availabilityResults = details
                            .Results
                            .Select(accommodationAvailability =>
                            {
                                var minPrice = accommodationAvailability.RoomContractSets.Min(r => r.Price.NetTotal);
                                var maxPrice = accommodationAvailability.RoomContractSets.Max(r => r.Price.NetTotal);
                                var hasDuplicates = accommodationDuplicates.Contains(new ProviderAccommodationId(providerKey, accommodationAvailability.AccommodationDetails.Id));
                                
                                var result = new AvailabilityResult(providerAvailability.Details.AvailabilityId,
                                    accommodationAvailability.AccommodationDetails,
                                    accommodationAvailability.RoomContractSets,
                                    minPrice,
                                    maxPrice,
                                    hasDuplicates);

                                return ProviderData.Create(providerKey, result);
                            })
                            .ToList();

                        return availabilityResults;
                    });
            }
        }
        
        
        public Task<Result<ProviderData<DeadlineDetails>, ProblemDetails>> GetDeadlineDetails(
            DataProviders dataProvider, string availabilityId, Guid roomContractSetId, string languageCode)
        {
            return GetDeadline()
                .Map(AddProviderData);

            Task<Result<DeadlineDetails, ProblemDetails>> GetDeadline() => _providerRouter.GetDeadline(dataProvider,
                availabilityId,
                roomContractSetId, languageCode);

            ProviderData<DeadlineDetails> AddProviderData(DeadlineDetails deadlineDetails)
                => ProviderData.Create(dataProvider, deadlineDetails);
        }
        
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);
        
        private readonly IAvailabilitySearchScheduler _searchScheduler;
        private readonly IAvailabilityStorage _storage;
        private readonly IAccommodationDuplicatesService _duplicatesService;
        private readonly IProviderRouter _providerRouter;
        private readonly IMemoryFlow _memoryFlow;
        
        private readonly struct AvailabilityWithTimestamp
        {
            [JsonConstructor]
            public AvailabilityWithTimestamp(AvailabilityDetails details, long timeStamp)
            {
                TimeStamp = timeStamp;
                Details = details;
            }
            
            public long TimeStamp { get; }
            public AvailabilityDetails Details { get; }
        }
    }
}