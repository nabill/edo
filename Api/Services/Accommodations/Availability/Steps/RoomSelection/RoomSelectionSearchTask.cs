using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
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
            => new(serviceProvider.GetRequiredService<IRoomSelectionPriceProcessor>(),
            serviceProvider.GetRequiredService<ISupplierConnectorManager>(),
            serviceProvider.GetRequiredService<IRoomSelectionStorage>()
        );


        public async Task<Result<SingleAccommodationAvailability, ProblemDetails>> GetSupplierAvailability(Guid searchId,
            string htId, Suppliers supplier, string supplierAccommodationCode, string availabilityId, AccommodationBookingSettings settings,
            AgentContext agent, string languageCode)
        {
            return await ExecuteRequest()
                .Bind(Convert)
                .Bind(ConvertCurrencies)
                .Map(ProcessPolicies)
                .Map(ApplyMarkups)
                .Map(AlignPrices)
                .Tap(SaveToCache);


            Task<Result<AccommodationAvailability, ProblemDetails>> ExecuteRequest() 
                => _supplierConnectorManager.Get(supplier).GetAvailability(availabilityId, supplierAccommodationCode, languageCode);


            Result<SingleAccommodationAvailability, ProblemDetails> Convert(AccommodationAvailability availabilityDetails)
            {
                var roomContractSets = availabilityDetails.RoomContractSets
                    .Select(r => r.ToRoomContractSet(supplier, r.IsDirectContract))
                    .ToList();
                
                return new SingleAccommodationAvailability(availabilityDetails.AvailabilityId,
                    availabilityDetails.CheckInDate,
                    roomContractSets,
                    htId);
            }


            Task<Result<SingleAccommodationAvailability, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailability availabilityDetails)
                => _priceProcessor.ConvertCurrencies(availabilityDetails, agent);


            SingleAccommodationAvailability ProcessPolicies(SingleAccommodationAvailability availabilityDetails)
                => RoomSelectionPolicyProcessor.Process(availabilityDetails, settings.CancellationPolicyProcessSettings);


            Task<SingleAccommodationAvailability> ApplyMarkups(SingleAccommodationAvailability response) 
                => _priceProcessor.ApplyMarkups(response, agent);
            
            
            async Task<SingleAccommodationAvailability> AlignPrices(SingleAccommodationAvailability response) 
                => await _priceProcessor.AlignPrices(response);


            Task SaveToCache(SingleAccommodationAvailability details) 
                => _roomSelectionStorage.SaveResult(searchId, htId, details, supplier);
        }
        
        
        private readonly IRoomSelectionPriceProcessor _priceProcessor;
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
    }
}