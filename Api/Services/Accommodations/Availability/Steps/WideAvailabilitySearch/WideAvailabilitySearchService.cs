using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilitySearchService : IWideAvailabilitySearchService
    {
        public WideAvailabilitySearchService(IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IWideAvailabilityStorage availabilityStorage, IServiceScopeFactory serviceScopeFactory, BookingAnalyticsService bookingAnalyticsService,
            IAvailabilitySearchAreaService searchAreaService, IDateTimeProvider dateTimeProvider, IWideAvailabilityAccommodationsStorage accommodationsStorage,
            ILogger<WideAvailabilitySearchService> logger)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _availabilityStorage = availabilityStorage;
            _serviceScopeFactory = serviceScopeFactory;
            _bookingAnalyticsService = bookingAnalyticsService;
            _searchAreaService = searchAreaService;
            _dateTimeProvider = dateTimeProvider;
            _accommodationsStorage = accommodationsStorage;
            _logger = logger;
        }
        
   
        public async Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            if (!request.HtIds.Any())
                return Result.Failure<Guid>($"{nameof(request.HtIds)} must not be empty");
            
            if (request.CheckInDate.Date < _dateTimeProvider.UtcToday())
                return Result.Failure<Guid>("Check in date must not be in the past");
            
            var searchId = Guid.NewGuid();
            
            Baggage.SetSearchId(searchId);
            _logger.LogMultiSupplierAvailabilitySearchStarted(request.CheckInDate.ToShortDateString(), request.CheckOutDate.ToShortDateString(),
                request.HtIds.ToArray(), request.Nationality, request.RoomDetails.Count);

            var (_, isFailure, searchArea, error) = await _searchAreaService.GetSearchArea(request.HtIds, languageCode);
            if (isFailure)
                return Result.Failure<Guid>(error);

            _bookingAnalyticsService.LogWideAvailabilitySearch(request, searchId, searchArea.Locations, agent, languageCode);
            
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            await StartSearch(searchId, request, searchSettings, searchArea.AccommodationCodes, agent, languageCode);
                
            return searchId;
        }


        public async Task<WideAvailabilitySearchState> GetState(Guid searchId, AgentContext agent)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var searchStates = await _availabilityStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            return WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);
        }

        
        public async Task<IEnumerable<WideAvailabilityResult>> GetResult(Guid searchId, AvailabilitySearchFilter options, AgentContext agent, string languageCode)
        {
            Baggage.SetSearchId(searchId);
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var suppliers = options.Suppliers is not null && options.Suppliers.Any()
                ? options.Suppliers.Intersect(searchSettings.EnabledConnectors).ToList()
                : searchSettings.EnabledConnectors;
            
            var supplierSearchResults = await _availabilityStorage.GetResults(searchId, suppliers);
            var htIds = supplierSearchResults
                .SelectMany(r => r.AccommodationAvailabilities.Select(a=>a.HtId))
                .ToList();

            await _accommodationsStorage.EnsureAccommodationsCached(htIds, languageCode);

            return CombineAvailabilities(supplierSearchResults);

            IEnumerable<WideAvailabilityResult> CombineAvailabilities(IEnumerable<(Suppliers ProviderKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)> availabilities)
            {
                if (availabilities == null || !availabilities.Any())
                    return Enumerable.Empty<WideAvailabilityResult>();

                var queryable = availabilities
                    .SelectMany(supplierResults =>
                    {
                        var (supplierKey, supplierAvailabilities) = supplierResults;
                        return supplierAvailabilities
                            .Select(pa => (Supplier: supplierKey, Availability: pa));
                    })
                    .OrderBy(r => r.Availability.Created)
                    .RemoveRepeatedAccommodations()
                    .Select(r =>
                    {
                        var (supplier, availability) = r;
                        var roomContractSets = availability.RoomContractSets
                            .Select(rs => rs.ApplySearchSettings(isSupplierVisible: searchSettings.IsSupplierVisible,
                                isDirectContractsVisible: searchSettings.IsDirectContractFlagVisible))
                            .ToList();

                        if (searchSettings.AprMode == AprMode.Hide)
                            roomContractSets = roomContractSets.Where(rcs => !rcs.IsAdvancePurchaseRate).ToList();

                        if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                            roomContractSets = roomContractSets.Where(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= _dateTimeProvider.UtcNow())
                                .ToList();

                        var accommodation = _accommodationsStorage.GetAccommodation(availability.HtId, languageCode);

                        return new WideAvailabilityResult(accommodation,
                            roomContractSets,
                            availability.MinPrice,
                            availability.MaxPrice,
                            availability.CheckInDate,
                            availability.CheckOutDate,
                            searchSettings.IsSupplierVisible
                                ? supplier
                                : (Suppliers?)null,
                            availability.HtId);
                    })
                    .AsQueryable();

                return options.ApplyTo(queryable);
            }
        }


        private async Task StartSearch(Guid searchId, AvailabilityRequest request, AccommodationBookingSettings searchSettings, Dictionary<Suppliers, List<SupplierCodeMapping>> accommodationCodes, AgentContext agent, string languageCode)
        {
            foreach (var supplier in searchSettings.EnabledConnectors)
            {
                if (!accommodationCodes.TryGetValue(supplier, out var supplierCodeMappings))
                {
                    await _availabilityStorage.SaveState(searchId, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(0), 0), supplier);
                    continue;
                }
                
                // Starting search tasks in a separate thread
                StartSearchTask(supplier, supplierCodeMappings);
            }


            void StartSearchTask(Suppliers supplier, List<SupplierCodeMapping> supplierCodeMappings)
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
        
        
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly BookingAnalyticsService _bookingAnalyticsService;
        private readonly IAvailabilitySearchAreaService _searchAreaService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWideAvailabilityAccommodationsStorage _accommodationsStorage;
        private readonly ILogger<WideAvailabilitySearchService> _logger;
    }
}