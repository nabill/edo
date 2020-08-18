using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionService : IRoomSelectionService
    {
        public RoomSelectionService(IDataProviderFactory dataProviderFactory,
            IWideAvailabilityStorage wideAvailabilityStorage,
            IOptions<DataProviderOptions> providerOptions,
            IAccommodationDuplicatesService duplicatesService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _dataProviderFactory = dataProviderFactory;
            _wideAvailabilityStorage = wideAvailabilityStorage;
            _duplicatesService = duplicatesService;
            _serviceScopeFactory = serviceScopeFactory;
            _providerOptions = providerOptions.Value;
        }


        public async Task<Result<AvailabilitySearchTaskState>> GetState(Guid searchId, Guid resultId)
        {
            var (_, isFailure, selectedResult, error) =  await GetSelectedResult(searchId, resultId);
            if (isFailure)
                return Result.Failure<AvailabilitySearchTaskState>(error);
            
            var providerAccommodationIds = new List<ProviderAccommodationId>
            {
                new ProviderAccommodationId(selectedResult.DataProvider, selectedResult.Result.AccommodationDetails.Id)
            };
            
            var otherProvidersAccommodations = await _duplicatesService.GetDuplicateReports(providerAccommodationIds);
            var dataProviders = otherProvidersAccommodations
                .Select(a => a.Key.DataProvider)
                .ToList();

            var results = await _wideAvailabilityStorage.GetStates(searchId, dataProviders);
            return WideAvailabilitySearchState.FromProviderStates(searchId, results).TaskState;
        }
        
        
        public async Task<Result<AccommodationDetails, ProblemDetails>> GetAccommodation(Guid searchId, Guid resultId, string languageCode)
        {
            var (_, isFailure, selectedResult, error) = await GetSelectedResult(searchId, resultId);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<AccommodationDetails>(error);
            
            return await _dataProviderFactory
                .Get(selectedResult.DataProvider)
                .GetAccommodation(selectedResult.Result.AccommodationDetails.Id, languageCode);
        }


        public async Task<Result<List<RoomContractSet>>> Get(Guid searchId, Guid resultId, AgentContext agent, string languageCode)
        {
            var (_, isFailure, selectedResults, error) = await GetSelectedWideAvailabilityResults(searchId, resultId);
            if (isFailure)
                return Result.Failure<List<RoomContractSet>>(error);
            
            var providerTasks = selectedResults
                .Select(GetProviderAvailability)
                .ToArray();

            await Task.WhenAll(providerTasks);

            return providerTasks
                .Select(task => task.Result)
                .Where(taskResult => taskResult.IsSuccess)
                .Select(taskResult => taskResult.Value)
                .SelectMany(accommodationAvailability => accommodationAvailability.Data.RoomContractSets)
                .ToList();


            async Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetProviderAvailability((DataProviders, AccommodationAvailabilityResult) wideAvailabilityResult)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var (source, result) = wideAvailabilityResult;

                return await RoomSelectionSearchTask
                    .Create(scope.ServiceProvider)
                    .GetProviderAvailability(searchId, resultId, source, result.AccommodationDetails.Id, result.AvailabilityId, agent, languageCode);
            }
            

            async Task<Result<List<(DataProviders Source, AccommodationAvailabilityResult Result)>>> GetSelectedWideAvailabilityResults(Guid searchId, Guid resultId)
            {
                var results = await GetWideAvailabilityResults(searchId);
                
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
        }


        private async Task<List<(DataProviders DataProvider, AccommodationAvailabilityResult Result)>> GetWideAvailabilityResults(Guid searchId)
        {
            return (await _wideAvailabilityStorage.GetResults(searchId, _providerOptions.EnabledProviders))
                .SelectMany(r => r.AccommodationAvailabilities.Select(acr => (Source: r.ProviderKey, Result: acr)))
                .ToList();
        }


        private async Task<Result<(DataProviders DataProvider, AccommodationAvailabilityResult Result)>> GetSelectedResult(Guid searchId, Guid resultId)
        {
            var result = (await GetWideAvailabilityResults(searchId))
                .SingleOrDefault(r => r.Result.Id == resultId);

            return result.Equals(default)
                ? Result.Failure<(DataProviders, AccommodationAvailabilityResult)>("Could not find selected availability")
                : result;
        }

        
        private readonly IDataProviderFactory _dataProviderFactory;
        private readonly IWideAvailabilityStorage _wideAvailabilityStorage;
        private readonly IAccommodationDuplicatesService _duplicatesService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DataProviderOptions _providerOptions;
    }
}