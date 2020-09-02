using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionSearchTask
    {
        private RoomSelectionSearchTask(IPriceProcessor priceProcessor,
            IDataProviderManager dataProviderManager,
            IRoomSelectionStorage roomSelectionStorage)
        {
            _priceProcessor = priceProcessor;
            _dataProviderManager = dataProviderManager;
            _roomSelectionStorage = roomSelectionStorage;
        }


        public static RoomSelectionSearchTask Create(IServiceProvider serviceProvider)
        {
            return new RoomSelectionSearchTask(
                serviceProvider.GetRequiredService<IPriceProcessor>(),
                serviceProvider.GetRequiredService<IDataProviderManager>(),
                serviceProvider.GetRequiredService<IRoomSelectionStorage>()
            );
        }
        
        
        public async Task<Result<ProviderData<AccommodationAvailability>, ProblemDetails>> GetProviderAvailability(Guid searchId,
            Guid resultId,
            DataProviders dataProvider,
            string accommodationId, string availabilityId, AgentContext agent,
            string languageCode)
        {
            return await ExecuteRequest()
                .Bind(ConvertCurrencies)
                .Map(ApplyMarkups)
                .Map(AddProviderData)
                .Tap(SaveToCache);


            Task SaveToCache(ProviderData<AccommodationAvailability> details) => _roomSelectionStorage.SaveResult(searchId, resultId, details.Data, details.Source);


            Task<Result<AccommodationAvailability, ProblemDetails>> ExecuteRequest()
                => _dataProviderManager.Get(dataProvider).GetAvailability(availabilityId, accommodationId, languageCode);


            Task<Result<AccommodationAvailability, ProblemDetails>> ConvertCurrencies(AccommodationAvailability availabilityDetails)
                => _priceProcessor.ConvertCurrencies(agent, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices, AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<AccommodationAvailability>> ApplyMarkups(AccommodationAvailability response) 
                => _priceProcessor.ApplyMarkups(agent, response, AvailabilityResultsExtensions.ProcessPrices);


            ProviderData<AccommodationAvailability> AddProviderData(DataWithMarkup<AccommodationAvailability> availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails.Data);
        }
        
        
        private readonly IPriceProcessor _priceProcessor;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
    }
}