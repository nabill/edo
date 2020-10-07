using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionService : IRoomSelectionService
    {
        public RoomSelectionService(IDataProviderManager dataProviderManager,
            IWideAvailabilityStorage wideAvailabilityStorage,
            IAccommodationDuplicatesService duplicatesService,
            IAgencySystemSettingsService agencySystemSettingsService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _dataProviderManager = dataProviderManager;
            _wideAvailabilityStorage = wideAvailabilityStorage;
            _duplicatesService = duplicatesService;
            _agencySystemSettingsService = agencySystemSettingsService;
            _serviceScopeFactory = serviceScopeFactory;
        }


        public async Task<Result<AvailabilitySearchTaskState>> GetState(Guid searchId, Guid resultId, AgentContext agent)
        {
            var (_, isFailure, selectedResult, error) =  await GetSelectedResult(searchId, resultId, agent);
            if (isFailure)
                return Result.Failure<AvailabilitySearchTaskState>(error);
            
            var providerAccommodationIds = new List<ProviderAccommodationId>
            {
                new ProviderAccommodationId(selectedResult.DataProvider, selectedResult.Result.Accommodation.Id)
            };
            
            var otherProvidersAccommodations = await _duplicatesService.GetDuplicateReports(providerAccommodationIds);
            var dataProviders = otherProvidersAccommodations
                .Select(a => a.Key.DataProvider)
                .ToList();

            var results = await _wideAvailabilityStorage.GetStates(searchId, dataProviders);
            return WideAvailabilitySearchState.FromProviderStates(searchId, results).TaskState;
        }
        
        
        public async Task<Result<Accommodation, ProblemDetails>> GetAccommodation(Guid searchId, Guid resultId, AgentContext agent, string languageCode)
        {
            var (_, isFailure, selectedResult, error) = await GetSelectedResult(searchId, resultId, agent);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<Accommodation>(error);
            
            return await _dataProviderManager
                .Get(selectedResult.DataProvider)
                .GetAccommodation(selectedResult.Result.Accommodation.Id, languageCode);
        }


        public async Task<Result<List<RoomContractSet>>> Get(Guid searchId, Guid resultId, AgentContext agent, string languageCode)
        {
            var (_, isFailure, selectedResults, error) = await GetSelectedWideAvailabilityResults(searchId, resultId, agent);
            if (isFailure)
                return Result.Failure<List<RoomContractSet>>(error);

            var (_, isAprFailure, aprSettings, aprError) = await _agencySystemSettingsService.GetAdvancedPurchaseRatesSettings(agent.AgencyId);
            if(isAprFailure)
                return Result.Failure<List<RoomContractSet>>(aprError);
            
            var providerTasks = selectedResults
                .Select(GetProviderAvailability)
                .ToArray();

            await Task.WhenAll(providerTasks);

            return providerTasks
                .Select(task => task.Result)
                .Where(taskResult => taskResult.IsSuccess)
                .Select(taskResult => taskResult.Value)
                .SelectMany(accommodationAvailability => accommodationAvailability.Data.RoomContractSets)
                .Where(AprFilter)
                .ToList();


            async Task<Result<ProviderData<AccommodationAvailability>, ProblemDetails>> GetProviderAvailability((DataProviders, AccommodationAvailabilityResult) wideAvailabilityResult)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var (source, result) = wideAvailabilityResult;

                return await RoomSelectionSearchTask
                    .Create(scope.ServiceProvider)
                    .GetProviderAvailability(searchId, resultId, source, result.Accommodation.Id, result.AvailabilityId, agent, languageCode);
            }
            

            async Task<Result<List<(DataProviders Source, AccommodationAvailabilityResult Result)>>> GetSelectedWideAvailabilityResults(Guid searchId, Guid resultId, AgentContext agent)
            {
                var results = await GetWideAvailabilityResults(searchId, agent);
                
                var selectedResult = results
                    .SingleOrDefault(r => r.Result.Id == resultId);

                if (selectedResult.Equals(default))
                    return Result.Failure<List<(DataProviders, AccommodationAvailabilityResult)>>("Could not find selected availability");

                // If there is no duplicate, we'll execute request to a single provider only
                if (string.IsNullOrWhiteSpace(selectedResult.Result.DuplicateReportId))
                    return new List<(DataProviders Source, AccommodationAvailabilityResult Result)> {selectedResult};

                return results
                    .Where(r => r.Result.DuplicateReportId == selectedResult.Result.DuplicateReportId)
                    .ToList();
            }


            bool AprFilter(RoomContractSet roomSet)
            {
                if (aprSettings == AprMode.NotDisplay && roomSet.IsAdvancedPurchaseRate)
                    return false;

                return true;
            }
        }


        private async Task<List<(DataProviders DataProvider, AccommodationAvailabilityResult Result)>> GetWideAvailabilityResults(Guid searchId, AgentContext agent)
        {
            return (await _wideAvailabilityStorage.GetResults(searchId, await _dataProviderManager.GetEnabled(agent)))
                .SelectMany(r => r.AccommodationAvailabilities.Select(acr => (Source: r.ProviderKey, Result: acr)))
                .ToList();
        }


        private async Task<Result<(DataProviders DataProvider, AccommodationAvailabilityResult Result)>> GetSelectedResult(Guid searchId, Guid resultId, AgentContext agent)
        {
            var result = (await GetWideAvailabilityResults(searchId, agent))
                .SingleOrDefault(r => r.Result.Id == resultId);

            return result.Equals(default)
                ? Result.Failure<(DataProviders, AccommodationAvailabilityResult)>("Could not find selected availability")
                : result;
        }

        
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IWideAvailabilityStorage _wideAvailabilityStorage;
        private readonly IAccommodationDuplicatesService _duplicatesService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
    }
}