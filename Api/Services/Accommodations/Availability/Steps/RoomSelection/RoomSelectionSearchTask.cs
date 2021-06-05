using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.SuppliersCatalog;
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
            => new(
            serviceProvider.GetRequiredService<IRoomSelectionPriceProcessor>(),
            serviceProvider.GetRequiredService<ISupplierConnectorManager>(),
            serviceProvider.GetRequiredService<IRoomSelectionStorage>()
        );


        public async Task<Result<SupplierData<AccommodationAvailability>, ProblemDetails>> GetSupplierAvailability(Guid searchId,
            Guid resultId,
            Suppliers supplier,
            string accommodationId, string availabilityId,
            AccommodationBookingSettings settings,
            string htId,
            AgentContext agent,
            string languageCode)
        {
            return await ExecuteRequest()
                .Bind(ConvertCurrencies)
                .Map(ProcessPolicies)
                .Map(ApplyMarkups)
                .Map(AddSupplierData)
                .Tap(SaveToCache);


            Task<Result<AccommodationAvailability, ProblemDetails>> ExecuteRequest()
                => _supplierConnectorManager.Get(supplier).GetAvailability(availabilityId, accommodationId, languageCode);


            Task<Result<AccommodationAvailability, ProblemDetails>> ConvertCurrencies(AccommodationAvailability availabilityDetails)
                => _priceProcessor.ConvertCurrencies(availabilityDetails, agent);


            AccommodationAvailability ProcessPolicies(AccommodationAvailability availabilityDetails)
                => RoomSelectionPolicyProcessor.Process(availabilityDetails, settings.CancellationPolicyProcessSettings);


            Task<AccommodationAvailability> ApplyMarkups(AccommodationAvailability response) 
                => _priceProcessor.ApplyMarkups(response, agent);


            SupplierData<AccommodationAvailability> AddSupplierData(AccommodationAvailability availabilityDetails)
                => SupplierData.Create(supplier, availabilityDetails);
            
            
            Task SaveToCache(SupplierData<AccommodationAvailability> details)
            {
                var availabilityData = details.Data;
                var result = new SingleAccommodationAvailability(availabilityData.AvailabilityId,
                    availabilityData.CheckInDate,
                    availabilityData.RoomContractSets,
                    htId);
                
                return _roomSelectionStorage.SaveResult(searchId, resultId, result, details.Source);
            }
        }
        
        
        private readonly IRoomSelectionPriceProcessor _priceProcessor;
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
    }
}