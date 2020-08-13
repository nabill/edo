using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step3
{
    public class ThirdStepAvailabilitySearchService : IThirdStepAvailabilitySearchService
    {
        public ThirdStepAvailabilitySearchService(IProviderRouter providerRouter,
            IPriceProcessor priceProcessor,
            IAvailabilityResultsCache availabilityResultsCache)
        {
            _providerRouter = providerRouter;
            _priceProcessor = priceProcessor;
            _availabilityResultsCache = availabilityResultsCache;
        }
        
        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline?>, ProblemDetails>> GetExactAvailability(
            DataProviders dataProvider, string availabilityId, Guid roomContractSetId, AgentContext agent, string languageCode)
        {
            return await ExecuteRequest()
                .Bind(ConvertCurrencies)
                .Map(ApplyMarkups)
                .Tap(SaveToCache)
                .Map(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline?, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetExactAvailability(dataProvider, availabilityId, roomContractSetId, languageCode);


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
                
                return _availabilityResultsCache.Set(dataProvider, DataWithMarkup.Create(responseWithDeadline.Data.Value, 
                    responseWithDeadline.Policies));
            }


            ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline?> AddProviderData(
                DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline?> availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails.Data);
        }
        
        private readonly IProviderRouter _providerRouter;
        private readonly IPriceProcessor _priceProcessor;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
    }
}