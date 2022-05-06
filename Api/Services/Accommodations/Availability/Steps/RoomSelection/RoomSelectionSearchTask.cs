using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Messaging;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;


namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionSearchTask
    {
        private RoomSelectionSearchTask(IRoomSelectionPriceProcessor priceProcessor, ISupplierConnectorManager supplierConnectorManager,
            IRoomSelectionStorage roomSelectionStorage, ISupplierOptionsStorage supplierOptionsStorage, IMessageBus messageBus)
        {
            _priceProcessor = priceProcessor;
            _supplierConnectorManager = supplierConnectorManager;
            _roomSelectionStorage = roomSelectionStorage;
            _supplierOptionsStorage = supplierOptionsStorage;
            _messageBus = messageBus;
        }


        public static RoomSelectionSearchTask Create(IServiceProvider serviceProvider)
            => new(priceProcessor: serviceProvider.GetRequiredService<IRoomSelectionPriceProcessor>(),
            supplierConnectorManager: serviceProvider.GetRequiredService<ISupplierConnectorManager>(),
            roomSelectionStorage: serviceProvider.GetRequiredService<IRoomSelectionStorage>(),
            supplierOptionsStorage: serviceProvider.GetRequiredService<ISupplierOptionsStorage>(),
            messageBus: serviceProvider.GetRequiredService<IMessageBus>()
        );


        public async Task<Result<SingleAccommodationAvailability, ProblemDetails>> GetSupplierAvailability(Guid searchId,
            string htId, string supplierCode, string supplierAccommodationCode, string availabilityId, AccommodationBookingSettings settings,
            AgentContext agent, string languageCode, string countryHtId, string localityHtId, int marketId, string countryCode)
        {
            return await ExecuteRequest()
                .Tap(Publish)
                .Bind(Convert)
                .Bind(ConvertCurrencies)
                .Map(ProcessPolicies)
                .Map(ApplyMarkups)
                .Map(AlignPrices)
                .Tap(SaveToCache);


            Task<Result<AccommodationAvailability, ProblemDetails>> ExecuteRequest()
                => _supplierConnectorManager.Get(supplierCode).GetAvailability(availabilityId, supplierAccommodationCode, languageCode);


            void Publish(AccommodationAvailability availabilityDetails)
            {
                _messageBus.Publish(MessageBusTopics.RoomSelection, new
                {
                    SearchId = searchId,
                    SupplierCode = supplierCode,
                    Availability = availabilityDetails,
                    agent.AgentId,
                    agent.AgencyId
                });
            }


            Result<SingleAccommodationAvailability, ProblemDetails> Convert(AccommodationAvailability availabilityDetails)
            {
                var (_, isFailure, supplier, error) = _supplierOptionsStorage.Get(supplierCode);
                if (isFailure)
                    return ProblemDetailsBuilder.Fail<SingleAccommodationAvailability>(error);

                var roomContractSets = availabilityDetails.RoomContractSets
                    .Select(r => r.ToRoomContractSet(supplier.Name, supplier.Code, r.IsDirectContract))
                    .ToList();

                return new SingleAccommodationAvailability(availabilityId: availabilityDetails.AvailabilityId,
                    checkInDate: availabilityDetails.CheckInDate,
                    roomContractSets: roomContractSets,
                    htId: htId,
                    countryHtId: countryHtId,
                    localityHtId: localityHtId,
                    marketId: marketId,
                    countryCode: countryCode,
                    supplierCode: supplierCode);
            }


            Task<Result<SingleAccommodationAvailability, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailability availabilityDetails)
                => _priceProcessor.ConvertCurrencies(availabilityDetails);


            SingleAccommodationAvailability ProcessPolicies(SingleAccommodationAvailability availabilityDetails)
                => RoomSelectionPolicyProcessor.Process(availabilityDetails, settings.CancellationPolicyProcessSettings);


            Task<SingleAccommodationAvailability> ApplyMarkups(SingleAccommodationAvailability response)
                => _priceProcessor.ApplyMarkups(response, agent);


            SingleAccommodationAvailability AlignPrices(SingleAccommodationAvailability response)
                => _priceProcessor.AlignPrices(response);


            Task SaveToCache(SingleAccommodationAvailability details)
                => _roomSelectionStorage.SaveResult(searchId, htId, details, supplierCode);
        }


        private readonly IRoomSelectionPriceProcessor _priceProcessor;
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        private readonly IMessageBus _messageBus;
    }
}