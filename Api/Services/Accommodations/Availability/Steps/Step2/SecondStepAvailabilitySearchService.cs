using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step1;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step2
{
    public class SecondStepAvailabilitySearchService : ISecondStepAvailabilitySearchService
    {
        public SecondStepAvailabilitySearchService(IProviderRouter providerRouter,
            IAvailabilityStorage availabilityStorage,
            IOptions<DataProviderOptions> providerOptions,
            IAccommodationDuplicatesService duplicatesService,
            IPriceProcessor priceProcessor)
        {
            _providerRouter = providerRouter;
            _availabilityStorage = availabilityStorage;
            _duplicatesService = duplicatesService;
            _priceProcessor = priceProcessor;
            _providerOptions = providerOptions.Value;
        }


        public async Task<AvailabilitySearchTaskState> GetState(Guid searchId, Guid resultId)
        {
            var selectedResult =  await GetSelectedResult(searchId, resultId);
            var providerAccommodationIds = new List<ProviderAccommodationId>
            {
                new ProviderAccommodationId(selectedResult.DataProvider, selectedResult.Result.AccommodationDetails.Id)
            };
            
            var otherProvidersAccommodations = await _duplicatesService.GetDuplicateReports(providerAccommodationIds);
            var dataProviders = otherProvidersAccommodations
                .Select(a => a.Key.DataProvider)
                .ToList();

            var results = await _availabilityStorage.GetProviderResults<ProviderAvailabilitySearchState>(searchId, dataProviders);
            return AvailabilitySearchState.FromProviderStates(searchId, results).TaskState;
        }
        
        
        public async Task<Result<AccommodationDetails, ProblemDetails>> GetAccommodation(Guid searchId, Guid resultId, string languageCode)
        {
            var selectedResult = await GetSelectedResult(searchId, resultId);
            return await _providerRouter.GetAccommodation(selectedResult.DataProvider, selectedResult.Result.AccommodationDetails.Id, languageCode);
        }


        public async Task<Result<List<ProviderData<RoomContractSet>>>> Get(Guid searchId, Guid resultId, AgentContext agent, string languageCode)
        {
            var providerTasks = (await GetFirstStepResults())
                .Select(r => GetProviderAvailability(r.Source, r.Result.AccommodationDetails.Id, r.Result.AvailabilityId, agent, languageCode))
                .ToArray();

            await Task.WhenAll(providerTasks);

            return providerTasks
                .Select(task => task.Result)
                .Where(taskResult => taskResult.IsSuccess)
                .Select(taskResult => taskResult.Value)
                .SelectMany(accommodationAvailability =>
                {
                    return accommodationAvailability.Data.RoomContractSets.Select(r =>
                        ProviderData.Create(accommodationAvailability.Source, r));
                })
                .ToList();


            async Task<List<(DataProviders Source, AccommodationAvailabilityResult Result)>> GetFirstStepResults()
            {
                var results = await GetSearchResults(searchId);
                var selectedResult = results
                    .Single(r => r.Result.Id == resultId);

                return results
                    .Where(r => r.Result.DuplicateReportId == selectedResult.Result.DuplicateReportId)
                    .ToList();
            }
        }


        private Task<(DataProviders DataProvider, AccommodationAvailabilityResult Result)[]> GetSearchResults(Guid searchId)
        {
            return _availabilityStorage.GetProviderResults<AccommodationAvailabilityResult>(searchId, _providerOptions.EnabledProviders);
        }


        private async Task<(DataProviders DataProvider, AccommodationAvailabilityResult Result)> GetSelectedResult(Guid searchId, Guid resultId)
        {
            return (await GetSearchResults(searchId))
                .Single(r => r.Result.Id == resultId);
        }

        private async Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetProviderAvailability(DataProviders dataProvider,
            string accommodationId, string availabilityId, AgentContext agent,
            string languageCode)
        {
            return await ExecuteRequest()
                .Bind(ConvertCurrencies)
                .Map(ApplyMarkups)
                .Map(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetAvailable(dataProvider, accommodationId, availabilityId, languageCode);


            Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailabilityDetails availabilityDetails)
                => _priceProcessor.ConvertCurrencies(agent, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices, AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<SingleAccommodationAvailabilityDetails>> ApplyMarkups(SingleAccommodationAvailabilityDetails response) 
                => _priceProcessor.ApplyMarkups(agent, response, AvailabilityResultsExtensions.ProcessPrices);


            ProviderData<SingleAccommodationAvailabilityDetails> AddProviderData(DataWithMarkup<SingleAccommodationAvailabilityDetails> availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails.Data);
        }
        
        
        private readonly IProviderRouter _providerRouter;
        private readonly IAvailabilityStorage _availabilityStorage;
        private readonly IAccommodationDuplicatesService _duplicatesService;
        private readonly IPriceProcessor _priceProcessor;
        private readonly DataProviderOptions _providerOptions;
    }
}