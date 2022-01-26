using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.SuppliersCatalog;
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
            IOptionsMonitor<SearchLimits> searchLimits)
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
            _searchLimits = searchLimits;
        }
        
   
        public async Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            if (!request.HtIds.Any())
                return Result.Failure<Guid>($"{nameof(request.HtIds)} must not be empty");
            
            if (request.HtIds.Count > _searchLimits.CurrentValue.HtIds)
                return Result.Failure<Guid>($"{nameof(request.HtIds)} limit exceeded");
            
            if (request.CheckInDate.Date < _dateTimeProvider.UtcToday())
                return Result.Failure<Guid>("Check in date must not be in the past");
            
            if (request.RoomDetails.Count > _searchLimits.CurrentValue.Rooms)
                return Result.Failure<Guid>($"{nameof(request.RoomDetails)} limit exceeded");
            
            if (request.RoomDetails.Any(r => r.AdultsNumber > _searchLimits.CurrentValue.AdultsPerRoom))
                return Result.Failure<Guid>("Adults per room limit exceeded");
            
            if (request.RoomDetails.Any(r => r.ChildrenAges?.Count > _searchLimits.CurrentValue.ChildrenPerRoom))
                return Result.Failure<Guid>("Children per room limit exceeded");

            var searchId = Guid.NewGuid();
            
            Baggage.AddSearchId(searchId);
            _logger.LogMultiSupplierAvailabilitySearchStarted(request.CheckInDate.ToShortDateString(), request.CheckOutDate.ToShortDateString(),
                request.HtIds.ToArray(), request.Nationality, request.RoomDetails.Count);

            var (_, isFailure, searchArea, error) = await _searchAreaService.GetSearchArea(request.HtIds, languageCode);
            if (isFailure)
                return Result.Failure<Guid>(error);

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
            return await _availabilityStorage.GetFilteredResults(searchId, options, searchSettings, suppliers, languageCode);
        }


        private async Task StartSearch(Guid searchId, AvailabilityRequest request, AccommodationBookingSettings searchSettings, Dictionary<Suppliers, List<SupplierCodeMapping>> accommodationCodes, AgentContext agent, string languageCode)
        {
            foreach (var supplier in searchSettings.EnabledConnectors)
            {
                if (!accommodationCodes.TryGetValue(supplier, out var supplierCodeMappings))
                {
                    await _stateStorage.SaveState(searchId, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(0), 0), supplier);
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
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBookingAnalyticsService _bookingAnalyticsService;
        private readonly IAvailabilitySearchAreaService _searchAreaService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAvailabilityRequestStorage _requestStorage;
        private readonly ILogger<WideAvailabilitySearchService> _logger;
        private readonly IOptionsMonitor<SearchLimits> _searchLimits;
    }
}