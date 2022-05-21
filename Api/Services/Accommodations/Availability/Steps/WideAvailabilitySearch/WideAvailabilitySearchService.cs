using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Metrics;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
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
            IWideAvailabilityAccommodationsStorage accommodationsStorage, IDistributedFlow flow)
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
            _flow = flow;
        }
        
   
        public async Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            if (!request.HtIds.Any())
                return Result.Failure<Guid>($"{nameof(request.HtIds)} must not be empty");
            
            if (request.CheckInDate.Date < _dateTimeProvider.UtcToday())
                return Result.Failure<Guid>("Check in date must not be in the past");

            var searchId = await GetSearchId(request);
            
            Baggage.AddSearchId(searchId);
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

            _bookingAnalyticsService.LogWideAvailabilitySearch(request, searchId, searchArea.Locations, agent, languageCode);
            
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            await _requestStorage.Set(searchId, request);
            await StartSearch(searchId, request, searchSettings, searchArea.AccommodationCodes, agent, languageCode);
                
            return searchId;
        }


        public async Task<WideAvailabilitySearchState> GetState(Guid searchId, AgentContext agent)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var searchStates = await _stateStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            return WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);
        }

        
        public async Task<IEnumerable<WideAvailabilityResult>> GetResult(Guid searchId, AvailabilitySearchFilter options, AgentContext agent, string languageCode)
        {
            Baggage.AddSearchId(searchId);
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var suppliers = options.Suppliers is not null && options.Suppliers.Any()
                ? options.Suppliers.Intersect(searchSettings.EnabledConnectors).ToList()
                : searchSettings.EnabledConnectors;

            var result = await _availabilityStorage.GetFilteredResults(searchId, options, searchSettings, suppliers, languageCode);
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

                return response.Select(r =>
                    {
                        var roomContractSets = r.RoomContractSets
                            .Where(roomSet => RoomContractSetSettingsChecker.IsDisplayAllowed(roomSet, r.CheckInDate, searchSettings,
                                _dateTimeProvider))
                            .ToList();
                        
                        return new WideAvailabilityResult(accommodation: _accommodationsStorage.GetAccommodation(r.HtId, languageCode),
                            roomContractSets: roomContractSets,
                            minPrice: roomContractSets.Min(r => r.Rate.FinalPrice.Amount),
                            maxPrice: roomContractSets.Max(r => r.Rate.FinalPrice.Amount),
                            checkInDate: r.CheckInDate,
                            checkOutDate: r.CheckOutDate,
                            supplierCode: searchSettings.IsSupplierVisible
                                ? r.SupplierCode
                                : null,
                            htId: r.HtId);
                    })
                    .ToList();
            }
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
                        .Start(searchId, request, supplierCodeMappings, supplier, agent, languageCode, searchSettings);
                });
            }
        }


        private async Task<Guid> GetSearchId(AvailabilityRequest request)
        {
            var key = _flow.BuildKey(nameof(WideAvailabilitySearchService), "SearchId", HashGenerator.ComputeHash(request));
            var searchId = await _flow.GetAsync<Guid>(key);
            
            if (Guid.Empty == searchId)
            {
                Counters.WideSearchCacheMissCounter.Inc();
                searchId = Guid.NewGuid();
                await _flow.SetAsync(key, searchId, TimeSpan.FromHours(SearchIdCacheHoursExpire));
            }
            else
            {
                Counters.WideSearchCacheHitCounter.Inc();
            }

            return searchId;
        }


        private const int SearchIdCacheHoursExpire = 1;


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
        private readonly IDistributedFlow _flow;
    }
}