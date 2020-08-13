using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step1
{
    public class FirstStepAvailabilitySearchService : IFirstStepAvailabilitySearchService
    {
        public FirstStepAvailabilitySearchService(IAvailabilitySearchScheduler searchScheduler, 
            IAvailabilityStorage storage,
            IAccommodationDuplicatesService duplicatesService,
            IProviderRouter providerRouter,
            IOptions<DataProviderOptions> providerOptions)
        {
            _searchScheduler = searchScheduler;
            _storage = storage;
            _duplicatesService = duplicatesService;
            _providerRouter = providerRouter;
            _providerOptions = providerOptions.Value;
        }
        
        public Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            return _searchScheduler.StartSearch(request, _providerOptions.EnabledProviders, agent, languageCode);
        }


        public async Task<AvailabilitySearchState> GetState(Guid searchId)
        {
            var providerSearchStates = await _storage.GetProviderResults<ProviderAvailabilitySearchState>(searchId, _providerOptions.EnabledProviders);
            var searchStates = providerSearchStates
                .Where(s => !s.Result.Equals(default))
                .ToDictionary(s => s.DataProvider, s => s.Result);
            
            return AvailabilitySearchState.FromProviderStates(searchId, searchStates);
        }
        
        public async Task<IEnumerable<AvailabilityResult>> GetResult(Guid searchId, AgentContext agent)
        {
            var accommodationDuplicates = await _duplicatesService.Get(agent);
            
            var providerSearchResults = (await _storage.GetProviderResults<AccommodationAvailabilityResult[]>(searchId, _providerOptions.EnabledProviders, true))
                .Where(t => !t.Result.Equals(default))
                .ToList();
            
            return CombineAvailabilities(providerSearchResults);


            IEnumerable<AvailabilityResult> CombineAvailabilities(List<(DataProviders ProviderKey, AccommodationAvailabilityResult[] AccommodationAvailabilities)> availabilities)
            {
                if (availabilities == null || !availabilities.Any())
                    return Enumerable.Empty<AvailabilityResult>();

                return availabilities
                    .SelectMany(providerResults =>
                    {
                        var (providerKey, providerAvailabilities) = providerResults;
                        return providerAvailabilities
                            .Select(pa => (Provider: providerKey, Availability: pa));
                    })
                    .OrderBy(r => r.Availability.Timestamp)
                    .Select(r =>
                    {
                        var (provider, availability) = r;
                        var providerAccommodationId = new ProviderAccommodationId(provider, availability.AccommodationDetails.Id);
                        var hasDuplicatesForCurrentAgent = accommodationDuplicates.Contains(providerAccommodationId);

                        return new AvailabilityResult(availability.Id,
                            availability.AccommodationDetails,
                            availability.RoomContractSets,
                            availability.MinPrice,
                            availability.MaxPrice,
                            hasDuplicatesForCurrentAgent);
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
        
        
        private readonly IAvailabilitySearchScheduler _searchScheduler;
        private readonly IAvailabilityStorage _storage;
        private readonly IAccommodationDuplicatesService _duplicatesService;
        private readonly IProviderRouter _providerRouter;
        private readonly DataProviderOptions _providerOptions;
    }
}