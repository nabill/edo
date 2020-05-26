using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityStorage : IAvailabilityStorage
    {
        public AvailabilityStorage(IDistributedFlow distributedFlow, IMemoryFlow memoryFlow, IOptions<DataProviderOptions> options)
        {
            _distributedFlow = distributedFlow;
            _memoryFlow = memoryFlow;
            _providerOptions = options.Value;
        }


        public Task SaveResult(Guid searchId, DataProviders dataProvider, AvailabilityDetails details) => SaveObject(searchId, dataProvider, details);


        public Task SetState(Guid searchId, DataProviders dataProvider, AvailabilitySearchState searchState)
            => SaveObject(searchId, dataProvider, searchState);


        public async Task<CombinedAvailabilityDetails> GetResult(Guid searchId, int skip, int top)
        {
            var key = _memoryFlow.BuildKey(nameof(AvailabilityStorage), searchId.ToString());
            if (!_memoryFlow.TryGetValue(key, out List<(DataProviders DataProvider, AvailabilityDetails Result)> providerSearchResults))
            {
                providerSearchResults = (await GetProviderResults<AvailabilityDetails>(searchId))
                    .Where(t => !t.Result.Equals(default))
                    .ToList();

                if ((await GetState(searchId)).TaskState == AvailabilitySearchTaskState.Completed)
                    _memoryFlow.Set(key, providerSearchResults, CacheExpirationTime);
            }

            return CombineAvailabilities(providerSearchResults, skip, top);


            static CombinedAvailabilityDetails CombineAvailabilities(List<(DataProviders ProviderKey, AvailabilityDetails Availability)> availabilities,
                int skip, int top)
            {
                if (availabilities == null || !availabilities.Any())
                    return CombinedAvailabilityDetails.Empty;

                var firstResult = availabilities.First().Availability;

                var results = availabilities
                    .SelectMany(providerResults =>
                    {
                        var (providerKey, providerAvailability) = providerResults;
                        var availabilityResults = providerAvailability
                            .Results
                            .Select(r =>
                            {
                                var result = new AvailabilityResult(providerAvailability.AvailabilityId,
                                    r.AccommodationDetails,
                                    r.RoomContractSets);

                                return ProviderData.Create(providerKey, result);
                            })
                            .ToList();

                        return availabilityResults;
                    })
                    .Skip(skip)
                    .Take(top)
                    .ToList();

                var processed = availabilities.Sum(a => a.Availability.NumberOfProcessedAccommodations);
                return new CombinedAvailabilityDetails(firstResult.NumberOfNights, firstResult.CheckInDate, firstResult.CheckOutDate, processed, results);
            }
        }


        public async Task<AvailabilitySearchState> GetState(Guid searchId)
        {
            var providerSearchStates = await GetProviderResults<AvailabilitySearchState>(searchId);
            var searchStates = providerSearchStates
                .Where(s => !s.Result.Equals(default))
                .Select(s => s.Result.TaskState)
                .ToHashSet();

            var totalResultsCount = GetResultsCount(providerSearchStates);
            var errors = GetErrors(providerSearchStates);

            if (searchStates.Count == 1)
                return AvailabilitySearchState.FromState(searchId, searchStates.Single(), totalResultsCount, errors);

            if (searchStates.Contains(AvailabilitySearchTaskState.Completed))
            {
                if (searchStates.Contains(AvailabilitySearchTaskState.Pending))
                    return AvailabilitySearchState.PartiallyCompleted(searchId, totalResultsCount, errors);

                return AvailabilitySearchState.Completed(searchId, totalResultsCount, errors);
            }

            throw new ArgumentException($"Invalid tasks state: {string.Join(";", searchStates)}");


            static string GetErrors((DataProviders DataProvider, AvailabilitySearchState Result)[] states)
            {
                var errors = states
                    .Select(p => p.Result.Error)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToArray();

                return string.Join("; ", errors);
            }


            static int GetResultsCount((DataProviders DataProvider, AvailabilitySearchState Result)[] states)
            {
                return states.Sum(s => s.Result.ResultCount);
            }
        }


        private Task<(DataProviders DataProvider, TObject Result)[]> GetProviderResults<TObject>(Guid searchId)
        {
            var providerTasks = _providerOptions
                .EnabledProviders
                .Select(async p =>
                {
                    var key = BuildKey<TObject>(searchId, p);
                    return (
                        ProviderKey: p,
                        Object: await _distributedFlow.GetAsync<TObject>(key)
                    );
                })
                .ToArray();

            return Task.WhenAll(providerTasks);
        }


        private Task SaveObject<TObjectType>(Guid searchId, DataProviders dataProvider, TObjectType @object)
        {
            var key = BuildKey<TObjectType>(searchId, dataProvider);
            return _distributedFlow.SetAsync(key, @object, CacheExpirationTime);
        }


        private string BuildKey<TObjectType>(Guid searchId, DataProviders dataProvider)
            => _distributedFlow.BuildKey(nameof(AvailabilityStorage),
                searchId.ToString(),
                typeof(TObjectType).Name,
                dataProvider.ToString());


        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);

        private readonly IDistributedFlow _distributedFlow;
        private readonly IMemoryFlow _memoryFlow;
        private readonly DataProviderOptions _providerOptions;
    }
}