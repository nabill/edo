using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilitySearchService : IWideAvailabilitySearchService
    {
        public WideAvailabilitySearchService(IAccommodationDuplicatesService duplicatesService,
            ILocationService locationService,
            IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IWideAvailabilityStorage availabilityStorage,
            IServiceScopeFactory serviceScopeFactory,
            IDateTimeProvider dateTimeProvider,
            AvailabilityAnalyticsService analyticsService,
            ILogger<WideAvailabilitySearchService> logger)
        {
            _duplicatesService = duplicatesService;
            _locationService = locationService;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _availabilityStorage = availabilityStorage;
            _serviceScopeFactory = serviceScopeFactory;
            _dateTimeProvider = dateTimeProvider;
            _analyticsService = analyticsService;
            _logger = logger;
        }
        
   
        public async Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            var searchId = Guid.NewGuid();
            _logger.LogMultiProviderAvailabilitySearchStarted($"Starting availability search with id '{searchId}'");
            
            var (_, isFailure, location, locationError) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Failure<Guid>(locationError.Detail);

            _analyticsService.LogWideAvailabilitySearch(request, searchId, location, agent, languageCode);
            
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            StartSearchTasks(searchId, request, searchSettings, location, agent, languageCode);
            
            return Result.Success(searchId);
        }


        public async Task<WideAvailabilitySearchState> GetState(Guid searchId, AgentContext agent)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var searchStates = await _availabilityStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            return WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);
        }
        
        public async Task<IEnumerable<WideAvailabilityResult>> GetResult(Guid searchId, AgentContext agent)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var accommodationDuplicates = await _duplicatesService.Get(agent);
            var supplierSearchResults = await _availabilityStorage.GetResults(searchId, searchSettings.EnabledConnectors);
            
            return CombineAvailabilities(supplierSearchResults);

            IEnumerable<WideAvailabilityResult> CombineAvailabilities(IEnumerable<(Suppliers ProviderKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)> availabilities)
            {
                if (availabilities == null || !availabilities.Any())
                    return Enumerable.Empty<WideAvailabilityResult>();

                return availabilities
                    .SelectMany(supplierResults =>
                    {
                        var (supplierKey, supplierAvailabilities) = supplierResults;
                        return supplierAvailabilities
                            .Select(pa => (Provider: supplierKey, Availability: pa));
                    })
                    .OrderBy(r => r.Availability.Timestamp)
                    .RemoveRepeatedAccommodations()
                    .Select(r =>
                    {
                        var (supplier, availability) = r;
                        var supplierAccommodationId = new SupplierAccommodationId(supplier, availability.Accommodation.Id);
                        var hasDuplicatesForCurrentAgent = accommodationDuplicates.Contains(supplierAccommodationId);
                        
                        return new WideAvailabilityResult(availability.Id,
                            availability.Accommodation,
                            availability.RoomContractSets,
                            availability.MinPrice,
                            availability.MaxPrice,
                            hasDuplicatesForCurrentAgent,
                            searchSettings.IsSupplierVisible 
                                ? supplier 
                                : (Suppliers?)null);
                    })
                    .Where(a => a.RoomContractSets.Any());
            }
        }

        private void StartSearchTasks(Guid searchId, AvailabilityRequest request, AccommodationBookingSettings searchSettings, Location location, AgentContext agent, string languageCode)
        {
            var contractsRequest = ConvertRequest(request, location);

            foreach (var supplier in GetSuppliersToSearch(location, searchSettings.EnabledConnectors))
            {
                Task.Run(async () =>
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    
                    await WideAvailabilitySearchTask
                        .Create(scope.ServiceProvider)
                        .Start(searchId, contractsRequest, supplier, agent, languageCode, searchSettings);
                });
            }


            IReadOnlyCollection<Suppliers> GetSuppliersToSearch(in Location location, List<Suppliers> suppliers)
            {
                return location.Suppliers != null && location.Suppliers.Any()
                    ? location.Suppliers.Intersect(suppliers).ToList()
                    : suppliers;
            }


            static EdoContracts.Accommodations.AvailabilityRequest ConvertRequest(in AvailabilityRequest request, in Location location)
            {
                var roomDetails = request.RoomDetails
                    .Select(r => new RoomOccupationRequest(r.AdultsNumber, r.ChildrenAges, r.Type, r.IsExtraBedNeeded))
                    .ToList();

                return new EdoContracts.Accommodations.AvailabilityRequest(request.Nationality, request.Residency, request.CheckInDate,
                    request.CheckOutDate,
                    request.Filters, roomDetails,
                    new EdoContracts.GeoData.Location(location.Name, location.Locality, location.Country, location.Coordinates, location.Distance,
                        location.Source, location.Type),
                    request.PropertyType, request.Ratings);
            }
        }
        
        
        private readonly IAccommodationDuplicatesService _duplicatesService;
        private readonly ILocationService _locationService;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly AvailabilityAnalyticsService _analyticsService;
        private readonly ILogger<WideAvailabilitySearchService> _logger;
    }
}