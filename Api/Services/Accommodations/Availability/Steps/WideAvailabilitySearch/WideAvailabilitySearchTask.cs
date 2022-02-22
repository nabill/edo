using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Metrics;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Hubs.Search;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using AvailabilityRequest = HappyTravel.EdoContracts.Accommodations.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilitySearchTask
    {
        private WideAvailabilitySearchTask(IWideAvailabilityStorage storage, IWideAvailabilityPriceProcessor priceProcessor,
            ISupplierConnectorManager supplierConnectorManager, IDateTimeProvider dateTimeProvider,
            ILogger<WideAvailabilitySearchTask> logger, IHubContext<SearchHub, ISearchHub> hubContext,
            IWideAvailabilitySearchStateStorage stateStorage, ISupplierOptionsStorage supplierOptionsStorage)
        {
            _storage = storage;
            _priceProcessor = priceProcessor;
            _supplierConnectorManager = supplierConnectorManager;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _hubContext = hubContext;
            _stateStorage = stateStorage;
            _supplierOptionsStorage = supplierOptionsStorage;
        }


        public static WideAvailabilitySearchTask Create(IServiceProvider serviceProvider)
            => new(storage: serviceProvider.GetRequiredService<IWideAvailabilityStorage>(),
                priceProcessor: serviceProvider.GetRequiredService<IWideAvailabilityPriceProcessor>(),
                supplierConnectorManager: serviceProvider.GetRequiredService<ISupplierConnectorManager>(),
                dateTimeProvider: serviceProvider.GetRequiredService<IDateTimeProvider>(),
                logger: serviceProvider.GetRequiredService<ILogger<WideAvailabilitySearchTask>>(),
                hubContext: serviceProvider.GetRequiredService<IHubContext<SearchHub, ISearchHub>>(),
                stateStorage: serviceProvider.GetRequiredService<IWideAvailabilitySearchStateStorage>(),
                supplierOptionsStorage:serviceProvider.GetRequiredService<ISupplierOptionsStorage>());


        public async Task Start(Guid searchId, Models.Availabilities.AvailabilityRequest availabilityRequest, 
            List<SupplierCodeMapping> accommodationCodeMappings, int supplierId, 
            AgentContext agent, string languageCode,
            AccommodationBookingSettings searchSettings)
        {
            var connectorRequest = CreateRequest(availabilityRequest, accommodationCodeMappings, searchSettings);
            var supplier = _supplierOptionsStorage.GetById(supplierId);
            var supplierConnector = _supplierConnectorManager.GetByCode(supplier.Code);
            using var _ = Counters.WideAccommodationAvailabilitySearchTaskDuration.WithLabels(supplier.Name).NewTimer();

            try
            {
                _logger.LogSupplierAvailabilitySearchStarted(searchId, supplier.Name);

                await GetAvailability(connectorRequest, languageCode)
                    .Map(Convert)
                    .Bind(ConvertCurrencies)
                    .Map(ProcessPolicies)
                    .Map(ApplyMarkups)
                    .Map(AlignPrices)
                    .Tap(SaveResult)
                    .Tap(NotifyClient)
                    .Finally(SaveState);
            }
            catch (Exception ex)
            {
                _logger.LogSupplierAvailabilitySearchException(supplier.ConnectorUrl, ex);
                var result = ProblemDetailsBuilder.Fail<List<AccommodationAvailabilityResult>>("Server error", HttpStatusCode.InternalServerError);
                await SaveState(result);
            }


            async Task<Result<EdoContracts.Accommodations.Availability, ProblemDetails>> GetAvailability(AvailabilityRequest request,
                string languageCode)
            {
                var saveToStorageTask = _stateStorage.SaveState(searchId, SupplierAvailabilitySearchState.Pending(searchId), supplierId);
                var getAvailabilityTask = supplierConnector.GetAvailability(request, languageCode);
                await Task.WhenAll(saveToStorageTask, getAvailabilityTask);

                return getAvailabilityTask.Result;
            }
            
            
            List<AccommodationAvailabilityResult> Convert(EdoContracts.Accommodations.Availability details)
            {
                var htIdMapping = accommodationCodeMappings
                    .ToDictionary(m => m.SupplierCode, m => (m.AccommodationHtId, m.CountryHtId, m.LocalityHtId));
                
                var now = _dateTimeProvider.UtcNow();
                return details
                    .Results
                    .Select(accommodationAvailability =>
                    {
                        var minPrice = accommodationAvailability.RoomContractSets.Min(r => r.Rate.FinalPrice.Amount);
                        var maxPrice = accommodationAvailability.RoomContractSets.Max(r => r.Rate.FinalPrice.Amount);

                        var roomContractSets = accommodationAvailability.RoomContractSets
                            .Select(rs => rs.ToRoomContractSet(supplier.Name, supplier.Id, supplier.Code, rs.IsDirectContract))
                            .Where(roomSet => RoomContractSetSettingsChecker.IsDisplayAllowed(roomSet, connectorRequest.CheckInDate, searchSettings,
                                _dateTimeProvider))
                            .ToList();

                        htIdMapping.TryGetValue(accommodationAvailability.AccommodationId, out var htId);
                        
                        return new AccommodationAvailabilityResult(searchId: searchId,
                            supplierId: supplierId,
                            created: now,
                            availabilityId: details.AvailabilityId,
                            roomContractSets: roomContractSets,
                            minPrice: minPrice,
                            maxPrice: maxPrice,
                            checkInDate: connectorRequest.CheckInDate,
                            checkOutDate: connectorRequest.CheckOutDate,
                            htId: htId.AccommodationHtId,
                            supplierAccommodationCode: accommodationAvailability.AccommodationId,
                            countryHtId: htId.CountryHtId,
                            localityHtId: htId.LocalityHtId);
                    })
                    .Where(a => !a.Equals(default) && a.RoomContractSets.Any())
                    .ToList();
            }


            Task<Result<List<AccommodationAvailabilityResult>, ProblemDetails>> ConvertCurrencies(List<AccommodationAvailabilityResult> availabilityDetails) 
                => _priceProcessor.ConvertCurrencies(availabilityDetails);


            Task<List<AccommodationAvailabilityResult>> ApplyMarkups(List<AccommodationAvailabilityResult> response) 
                => _priceProcessor.ApplyMarkups(response, agent);
            
            
            List<AccommodationAvailabilityResult> AlignPrices(List<AccommodationAvailabilityResult> response) 
                => _priceProcessor.AlignPrices(response);


            List<AccommodationAvailabilityResult> ProcessPolicies(List<AccommodationAvailabilityResult> response) 
                => WideAvailabilityPolicyProcessor.Process(response, searchSettings.CancellationPolicyProcessSettings);


            Task SaveResult(List<AccommodationAvailabilityResult> results) 
                => _storage.SaveResults(searchId, supplierId, results);


            Task NotifyClient() 
                => _hubContext.Clients.Group(agent.AgentId.ToString()).SearchStateChanged(new SearchStateChangedMessage(searchId));


            Task SaveState(Result<List<AccommodationAvailabilityResult>, ProblemDetails> result)
            {
                var state = result.IsSuccess
                    ? SupplierAvailabilitySearchState.Completed(searchId, result.Value.Select(r => r.HtId).ToList(), result.Value.Count)
                    : SupplierAvailabilitySearchState.Failed(searchId, result.Error.Detail);

                if (state.TaskState == AvailabilitySearchTaskState.Completed)
                {
                    _logger.LogSupplierAvailabilitySearchSuccess(searchId, supplier.Name, state.ResultCount);
                }
                else
                {
                    _logger.LogSupplierAvailabilitySearchFailure(searchId, supplier.Name, state.TaskState, state.Error);
                }

                return _stateStorage.SaveState(searchId, state, supplierId);
            }
        }
        
        
        private static AvailabilityRequest CreateRequest(Models.Availabilities.AvailabilityRequest request, List<SupplierCodeMapping> mappings,
            AccommodationBookingSettings searchSettings)
        {
            var roomDetails = request.RoomDetails
                .Select(r => new RoomOccupationRequest(r.AdultsNumber, r.ChildrenAges, r.Type, r.IsExtraBedNeeded))
                .ToList();

            var searchFilters = Convert(request.Filters);
            var supplierAccommodationCodes = mappings.Select(m => m.SupplierCode).ToList();
            
            return new AvailabilityRequest(nationality: request.Nationality,
                residency: request.Residency, 
                checkInDate: request.CheckInDate,
                checkOutDate: request.CheckOutDate, 
                filters: searchFilters | searchSettings.AdditionalSearchFilters,
                rooms: roomDetails, 
                propertyTypes: request.PropertyType,
                ratings: request.Ratings,
                accommodationIds: supplierAccommodationCodes);


            static EdoContracts.General.Enums.SearchFilters Convert(ClientSearchFilters filters)
            {
                EdoContracts.General.Enums.SearchFilters resultedFilter = default;

                if (filters.HasFlag(ClientSearchFilters.AvailableOnly))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.AvailableOnly;

                if (filters.HasFlag(ClientSearchFilters.AvailableWeighted))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.AvailableWeighted;

                if (filters.HasFlag(ClientSearchFilters.BestArrangement))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.BestArrangement;

                if (filters.HasFlag(ClientSearchFilters.BestContract))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.BestContract;

                if (filters.HasFlag(ClientSearchFilters.BestPrice))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.BestPrice;

                if (filters.HasFlag(ClientSearchFilters.ExcludeDynamic))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.ExcludeDynamic;

                if (filters.HasFlag(ClientSearchFilters.BestRoomPlans))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.BestRoomPlans;

                if (filters.HasFlag(ClientSearchFilters.ExcludeNonRefundable))
                    resultedFilter |= EdoContracts.General.Enums.SearchFilters.ExcludeNonRefundable;

                return resultedFilter;
            }
        }


        private readonly IWideAvailabilityPriceProcessor _priceProcessor;
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<WideAvailabilitySearchTask> _logger;
        private readonly IWideAvailabilityStorage _storage;
        private readonly IHubContext<SearchHub, ISearchHub> _hubContext;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}