using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilitySearchService : IWideAvailabilitySearchService
    {
        public WideAvailabilitySearchService(IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IWideAvailabilityStorage availabilityStorage, IServiceScopeFactory serviceScopeFactory, IBookingAnalyticsService bookingAnalyticsService,
            IAvailabilitySearchAreaService searchAreaService, IDateTimeProvider dateTimeProvider, IAvailabilityRequestStorage requestStorage,
            ILogger<WideAvailabilitySearchService> logger, IWideAvailabilitySearchStateStorage stateStorage,
            ISupplierOptionsStorage supplierOptionsStorage, IOptionsMonitor<SearchLimits> searchLimits, IWideAvailabilityPriceProcessor priceProcessor,
            IWideAvailabilityAccommodationsStorage accommodationsStorage, IAgentContextService agentContextService,
            IOptionsMonitor<SearchOptions> searchOptions)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _availabilityStorage = availabilityStorage;
            _serviceScopeFactory = serviceScopeFactory;
            _bookingAnalyticsService = bookingAnalyticsService;
            _searchAreaService = searchAreaService;
            _dateTimeProvider = dateTimeProvider;
            _requestStorage = requestStorage;
            _logger = logger;
            _stateStorage = stateStorage;
            _supplierOptionsStorage = supplierOptionsStorage;
            _searchLimits = searchLimits;
            _priceProcessor = priceProcessor;
            _accommodationsStorage = accommodationsStorage;
            _agentContextService = agentContextService;
            _searchOptions = searchOptions.CurrentValue;
        }


        public async Task<Result<Guid>> StartSearch(AvailabilityRequest request, string languageCode)
        {
            if (!request.HtIds.Any())
                return Result.Failure<Guid>($"{nameof(request.HtIds)} must not be empty");

            if (request.CheckInDate.Date < _dateTimeProvider.UtcToday())
                return Result.Failure<Guid>("Check in date must not be in the past");

            var searchId = await GetSearchId(request);

            Baggages.AddSearchId(searchId);
            _logger.LogMultiSupplierAvailabilitySearchStarted(request.CheckInDate.ToShortDateString(), request.CheckOutDate.ToShortDateString(),
                request.HtIds.ToArray(), request.Nationality, request.RoomDetails.Count);

            var (_, isFailure, searchArea, error) = await _searchAreaService.GetSearchArea(request.HtIds, languageCode);
            if (isFailure)
                return Result.Failure<Guid>(error);

            var searchLimitsValidator = new WideAvailabilitySearchLimitsValidator(_searchLimits.CurrentValue, searchArea.Locations);
            // Validator doesn't have async methods
            // ReSharper disable once MethodHasAsyncOverload
            var validationResult = searchLimitsValidator.Validate(request);

            if (!validationResult.IsValid)
                return Result.Failure<Guid>(validationResult.ToString("; "));

            var agent = await _agentContextService.GetAgent();
            _bookingAnalyticsService.LogWideAvailabilitySearch(agent);

            var searchSettings = await _accommodationBookingSettingsService.Get();
            await _requestStorage.Set(searchId, request);
            await StartSearch(searchId, request, searchSettings, searchArea.AccommodationCodes, agent, languageCode);

            return searchId;
        }


        public async Task<WideAvailabilitySearchState> GetState(Guid searchId)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get();
            var searchStates = await _stateStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            var directContractSuppliersCodes = GetDirectContractSuppliersCodes();
            var needFilterNonDirectContracts = await GetNeedFilterNonDirectContracts(searchId, searchStates, directContractSuppliersCodes);
            if (needFilterNonDirectContracts)
            {
                searchStates = searchStates
                    .Select(searchState => directContractSuppliersCodes.Contains(searchState.SupplierCode)
                        ? searchState
                        : (searchState.SupplierCode, SupplierAvailabilitySearchState.Pending(searchId))).ToList();
            }

            return WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);
        }


        public async Task<IEnumerable<WideAvailabilityResult>> GetResult(Guid searchId, AvailabilitySearchFilter? filter, string languageCode)
        {
            Baggages.AddSearchId(searchId);
            var agent = await _agentContextService.GetAgent();
            var searchSettings = await _accommodationBookingSettingsService.Get();
            
            var searchStates = await _stateStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            var directContractSuppliersCodes = GetDirectContractSuppliersCodes();
            var needFilterNonDirectContracts = await GetNeedFilterNonDirectContracts(searchId, searchStates, directContractSuppliersCodes);

            var suppliers = filter?.Suppliers is not null && filter.Suppliers.Any()
                ? filter.Suppliers.Intersect(searchSettings.EnabledConnectors).ToList()
                : searchSettings.EnabledConnectors;
            
            var result = await _availabilityStorage.GetFilteredResults(searchId, filter, searchSettings, suppliers, needFilterNonDirectContracts, directContractSuppliersCodes);
            var (isSuccess, _, results, error) = await ConvertCurrencies(result)
                .Map(ProcessPolicies)
                .Map(ApplyMarkups)
                .Map(AlignPrices)
                .Map(Convert);

            return isSuccess
                ? results
                : Array.Empty<WideAvailabilityResult>();


            Task<Result<List<AccommodationAvailabilityResult>, ProblemDetails>> ConvertCurrencies(List<AccommodationAvailabilityResult> availabilityDetails)
                => _priceProcessor.ConvertCurrencies(availabilityDetails);


            Task<List<AccommodationAvailabilityResult>> ApplyMarkups(List<AccommodationAvailabilityResult> response)
                => _priceProcessor.ApplyMarkups(response, agent);


            Task<List<AccommodationAvailabilityResult>> AlignPrices(List<AccommodationAvailabilityResult> response)
                => _priceProcessor.AlignPrices(response, agent);


            List<AccommodationAvailabilityResult> ProcessPolicies(List<AccommodationAvailabilityResult> response)
                => WideAvailabilityPolicyProcessor.Process(response, searchSettings.CancellationPolicyProcessSettings);


            async Task<List<WideAvailabilityResult>> Convert(List<AccommodationAvailabilityResult> response)
            {
                var htIds = response.Select(r => r.HtId).ToList();
                await _accommodationsStorage.EnsureAccommodationsCached(htIds, languageCode);

                var results = response.Select(r =>
                    {
                        var roomContractSets = r.RoomContractSets
                            .Where(roomSet => RoomContractSetSettingsChecker.IsDisplayAllowed(roomSet, r.CheckInDate, searchSettings,
                                _dateTimeProvider))
                            .ToList();

                        if (roomContractSets is null)
                            roomContractSets = new List<RoomContractSet>();

                        return new WideAvailabilityResult(accommodation: _accommodationsStorage.GetAccommodation(r.HtId, languageCode),
                            roomContractSets: roomContractSets,
                            minPrice: (roomContractSets.Count > 0)
                                ? roomContractSets.Min(r => r.Rate.FinalPrice.Amount)
                                : default,
                            maxPrice: (roomContractSets.Count > 0)
                                ? roomContractSets.Max(r => r.Rate.FinalPrice.Amount)
                                : default,
                            checkInDate: r.CheckInDate,
                            checkOutDate: r.CheckOutDate,
                            expiredAfter: r.ExpiredAfter,
                            supplierCode: searchSettings.IsSupplierVisible
                                ? r.SupplierCode
                                : null,
                            htId: r.HtId);
                    })
                    .ToList();

                return results
                    .Where(w => w.RoomContractSets.Count > 0)
                    .ToList();
            }
        }


        private async Task<bool> GetNeedFilterNonDirectContracts(Guid searchId, List<(string SupplierCode, SupplierAvailabilitySearchState States)> searchStates, List<string> directContractSuppliersCodes)
        {
            var anyDirectContractResultsArePending =
                searchStates.Any(s =>
                    directContractSuppliersCodes.Contains(s.SupplierCode)
                    && s.States.TaskState == AvailabilitySearchTaskState.Pending);

            var startedTimeResult = await _requestStorage.GetStartedTime(searchId);
            var needFilterNonDirectContracts = startedTimeResult.IsSuccess
                && anyDirectContractResultsArePending
                && _dateTimeProvider.UtcNow() - startedTimeResult.Value < TimeSpan.FromSeconds(_searchOptions.NonDirectResultsMaxDelaySeconds);
            return needFilterNonDirectContracts;
        }


        private List<string> GetDirectContractSuppliersCodes()
        {
            var supplierOptionsResult = _supplierOptionsStorage.GetAll();
            return supplierOptionsResult.IsSuccess ? supplierOptionsResult.Value.Where(s => s.IsDirectContract).Select(s => s.Code).ToList() : new List<string>();
        }


        private async Task StartSearch(Guid searchId, AvailabilityRequest request, AccommodationBookingSettings searchSettings, Dictionary<string, List<SupplierCodeMapping>> accommodationCodes, AgentContext agent, string languageCode)
        {
            foreach (var supplierCode in searchSettings.EnabledConnectors)
            {
                var (_, isFailure, supplier, _) = _supplierOptionsStorage.Get(supplierCode);
                if (isFailure || !accommodationCodes.TryGetValue(supplier.Code, out var supplierCodeMappings))
                {
                    await _stateStorage.SaveState(searchId, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(0), 0), supplier.Code);
                    continue;
                }

                // Starting search tasks in a separate thread
                StartSearchTask(supplier, supplierCodeMappings);
            }


            void StartSearchTask(SlimSupplier supplier, List<SupplierCodeMapping> supplierCodeMappings)
            {
                Task.Run(async () =>
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    await WideAvailabilitySearchTask
                        .Create(scope.ServiceProvider)
                        .Start(searchId, request, supplierCodeMappings, supplier, agent, languageCode, searchSettings, false);
                });
            }
        }


        private async Task<Guid> GetSearchId(AvailabilityRequest request)
        {
            if (!_searchOptions.IsCachedSearchEnabled)
                return Guid.NewGuid();

            var searchId = await _availabilityStorage.GetSearchId(HashGenerator.ComputeHash(request));
            return searchId == Guid.Empty
                ? Guid.NewGuid()
                : searchId;
        }


        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBookingAnalyticsService _bookingAnalyticsService;
        private readonly IAvailabilitySearchAreaService _searchAreaService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAvailabilityRequestStorage _requestStorage;
        private readonly ILogger<WideAvailabilitySearchService> _logger;
        private readonly IOptionsMonitor<SearchLimits> _searchLimits;
        private readonly IWideAvailabilityPriceProcessor _priceProcessor;
        private readonly IWideAvailabilityAccommodationsStorage _accommodationsStorage;
        private readonly IAgentContextService _agentContextService;
        private readonly SearchOptions _searchOptions;
    }
}