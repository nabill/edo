using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionSearchTask
    {
        private RoomSelectionSearchTask(IRoomSelectionPriceProcessor priceProcessor,
            ISupplierConnectorManager supplierConnectorManager,
            IRoomSelectionStorage roomSelectionStorage)
        {
            _priceProcessor = priceProcessor;
            _supplierConnectorManager = supplierConnectorManager;
            _roomSelectionStorage = roomSelectionStorage;
        }


        public static RoomSelectionSearchTask Create(IServiceProvider serviceProvider)
        {
            return new RoomSelectionSearchTask(
                serviceProvider.GetRequiredService<IRoomSelectionPriceProcessor>(),
                serviceProvider.GetRequiredService<ISupplierConnectorManager>(),
                serviceProvider.GetRequiredService<IRoomSelectionStorage>()
            );
        }
        
        
        public async Task<Result<SupplierData<AccommodationAvailability>, ProblemDetails>> GetProviderAvailability(Guid searchId,
            Guid resultId,
            Suppliers supplier,
            string accommodationId, string availabilityId, AgentContext agent,
            string languageCode)
        {
            return await ExecuteRequest()
                .Bind(ConvertCurrencies)
                .Map(ApplyMarkups)
                .Map(AddProviderData)
                .Tap(SaveToCache);


            Task SaveToCache(SupplierData<AccommodationAvailability> details) => _roomSelectionStorage.SaveResult(searchId, resultId, details.Data, details.Source);


            Task<Result<AccommodationAvailability, ProblemDetails>> ExecuteRequest()
                => _supplierConnectorManager.Get(supplier).GetAvailability(availabilityId, accommodationId, languageCode);


            Task<Result<AccommodationAvailability, ProblemDetails>> ConvertCurrencies(AccommodationAvailability availabilityDetails)
                => _priceProcessor.ConvertCurrencies(availabilityDetails, agent);


            Task<AccommodationAvailability> ApplyMarkups(AccommodationAvailability response) 
                => _priceProcessor.ApplyMarkups(response, agent);


            SupplierData<AccommodationAvailability> AddProviderData(AccommodationAvailability availabilityDetails)
                => SupplierData.Create(supplier, availabilityDetails);
        }
        
        
        private readonly IRoomSelectionPriceProcessor _priceProcessor;
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
    }
}