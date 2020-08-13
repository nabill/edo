using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step3
{
    public class ThirdStepAvailabilitySearchService : IThirdStepAvailabilitySearchService
    {
        public ThirdStepAvailabilitySearchService(IProviderRouter providerRouter,
            IPriceProcessor priceProcessor,
            IAvailabilityStorage availabilityStorage,
            IOptions<DataProviderOptions> providerOptions,
            IAvailabilityResultsCache availabilityResultsCache)
        {
            _providerRouter = providerRouter;
            _priceProcessor = priceProcessor;
            _availabilityStorage = availabilityStorage;
            _availabilityResultsCache = availabilityResultsCache;
            _providerOptions = providerOptions.Value;
        }
        
        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline?>, ProblemDetails>> GetExactAvailability(
            Guid searchId, Guid roomContractSetId, AgentContext agent, string languageCode)
        {
            // TODO: Rewrite using pipelines and reimplement AvailabilityResultsCache
            var (_, isFailure, result, error) = await GetCached(searchId, roomContractSetId);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline?>>(error);
            
            return await ExecuteRequest(result)
                .Bind(ConvertCurrencies)
                .Map(ApplyMarkups)
                .Tap(SaveToCache)
                .Map(AddProviderData);


            async Task<Result<(DataProviders DataProvider, RoomContractSet, string)>> GetCached(Guid searchId, Guid roomContractSetId)
            {
                var result = (await _availabilityStorage.GetProviderResults<SingleAccommodationAvailabilityDetails>(searchId, _providerOptions.EnabledProviders, true))
                    .SelectMany(r =>
                    {
                        return r.Result.RoomContractSets
                            .Select(rs => (Source: r.DataProvider, RoomContractSet: rs, AvailabilityId: r.Result.AvailabilityId));
                    })
                    .SingleOrDefault(r => r.RoomContractSet.Id == roomContractSetId);

                if (result.Equals(default))
                    return Result.Failure<(DataProviders, RoomContractSet, string)>("Could not find selected room contract set");

                return result;
            }

            
            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline?, ProblemDetails>> ExecuteRequest((DataProviders, RoomContractSet, string) selectedSet)
            {
                var (provider, roomContractSet, availabilityId) = selectedSet;
                return _providerRouter.GetExactAvailability(provider, availabilityId, roomContractSet.Id, languageCode);
            }


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline?, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailabilityDetailsWithDeadline? availabilityDetails) => _priceProcessor.ConvertCurrencies(agent,
                availabilityDetails,
                AvailabilityResultsExtensions.ProcessPrices,
                AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline?>>
                ApplyMarkups(SingleAccommodationAvailabilityDetailsWithDeadline? response)
                => _priceProcessor.ApplyMarkups(agent, response, AvailabilityResultsExtensions.ProcessPrices);


            Task SaveToCache(DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline?> responseWithDeadline)
            {
                if(!responseWithDeadline.Data.HasValue)
                    return Task.CompletedTask;
                
                return _availabilityResultsCache.Set(result.DataProvider, DataWithMarkup.Create(responseWithDeadline.Data.Value, 
                    responseWithDeadline.Policies));
            }


            ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline?> AddProviderData(
                DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline?> availabilityDetails)
                => ProviderData.Create(result.DataProvider, availabilityDetails.Data);
        }
        
        private readonly IProviderRouter _providerRouter;
        private readonly IPriceProcessor _priceProcessor;
        private readonly IAvailabilityStorage _availabilityStorage;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly DataProviderOptions _providerOptions;
    }
}