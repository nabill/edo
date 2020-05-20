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
    public class AvailabilityStorage
    {
        public AvailabilityStorage(IDistributedFlow distributedFlow, IOptions<DataProviderOptions> options)
        {
            _distributedFlow = distributedFlow;
            _providerOptions = options.Value;
        }


        public Task SaveResult(Guid searchId, DataProviders dataProvider, AvailabilityDetails details) => SaveObject(searchId, dataProvider, details);


        public Task SaveState(Guid searchId, DataProviders dataProvider, AvailabilitySearchState searchState)
            => SaveObject(searchId, dataProvider, searchState);


        public async Task<CombinedAvailabilityDetails> GetResult(Guid searchId, int page, int pageSize)
        {
            var providerSearchResults = (await GetProviderResults<AvailabilityDetails>(searchId))
                .Where(t => !t.Result.Equals(default))
                .ToList();

            return CombineAvailabilities(providerSearchResults, page, pageSize);


            static CombinedAvailabilityDetails CombineAvailabilities(List<(DataProviders ProviderKey, AvailabilityDetails Availability)> availabilities, int page, int pageSize)
            {
                if (availabilities == null || !availabilities.Any())
                    return new CombinedAvailabilityDetails(default, default, default, default, default);

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
                    .Skip(pageSize * page)
                    .Take(pageSize)
                    .ToList();

                var processed = availabilities.Sum(a => a.Availability.NumberOfProcessedAccommodations);
                return new CombinedAvailabilityDetails(firstResult.NumberOfNights, firstResult.CheckInDate, firstResult.CheckOutDate, processed, results);
            }
        }


        public async Task<AvailabilitySearchState> GetState(Guid searchId)
        {
            var providerSearchStates = await GetProviderResults<AvailabilitySearchState>(searchId);
            var searchStates = providerSearchStates
                .Select(s => s.Result.TaskState)
                .ToHashSet();

            if (searchStates.All(s => s == AvailabilitySearchTaskState.Failed))
            {
                var error = string.Join(";", providerSearchStates.Select(p => p.Result.Error).ToArray());
                return AvailabilitySearchState.Failed(searchId, error);
            }

            var totalResultsCount = providerSearchStates.Sum(s => s.Result.ResultCount);
            if (searchStates.Count == 1)
                return AvailabilitySearchState.FromState(searchId, searchStates.Single(), totalResultsCount);

            if (searchStates.Any(s => s == AvailabilitySearchTaskState.Completed))
                return AvailabilitySearchState.PartiallyCompleted(searchId, totalResultsCount);

            throw new ArgumentException($"Invalid tasks state: {string.Join(";", searchStates)}");
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
        private readonly DataProviderOptions _providerOptions;
    }
}