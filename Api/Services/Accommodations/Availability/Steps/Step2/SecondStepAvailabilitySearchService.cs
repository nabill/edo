using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step2
{
    public class SecondStepAvailabilitySearchService : ISecondStepAvailabilitySearchService
    {
        public SecondStepAvailabilitySearchService(IProviderRouter providerRouter,
            IPriceProcessor priceProcessor)
        {
            _providerRouter = providerRouter;
            _priceProcessor = priceProcessor;
        }
        
        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetAvailable(DataProviders dataProvider,
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
        private readonly IPriceProcessor _priceProcessor;
    }
}