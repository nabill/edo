using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.DirectApi.Extensions;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.DependencyInjection;
using AvailabilityRequest = HappyTravel.Edo.DirectApi.Models.AvailabilityRequest;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class WideSearchService
    {
        public WideSearchService(IAccommodationBookingSettingsService accommodationBookingSettingsService, 
            IWideAvailabilitySearchStateStorage stateStorage, IServiceScopeFactory serviceScopeFactory, 
            IAvailabilitySearchAreaService searchAreaService, IWideAvailabilityStorage availabilityStorage)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _stateStorage = stateStorage;
            _serviceScopeFactory = serviceScopeFactory;
            _searchAreaService = searchAreaService;
            _availabilityStorage = availabilityStorage;
        }


        public async Task<Result<StartSearchResponse>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode)
        {
            if (!request.HtIds.Any())
                return Result.Failure<StartSearchResponse>($"{nameof(request.HtIds)} must not be empty");
            
            if (request.CheckInDate.Date < DateTime.UtcNow.Date)
                return Result.Failure<StartSearchResponse>("Check in date must not be in the past");
            
            var searchId = Guid.NewGuid();

            var (_, isFailure, searchArea, error) = await _searchAreaService.GetSearchArea(request.HtIds, languageCode);
            if (isFailure)
                return Result.Failure<StartSearchResponse>(error);
            
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            await StartSearch(searchId, request, searchSettings, searchArea.AccommodationCodes, agent, languageCode);

            return new StartSearchResponse(searchId);
        }


        public async Task<Result<WideSearchResult>> GetResult(Guid searchId, AgentContext agent)
        {
            var isComplete = await IsComplete(searchId, agent);
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var result = await _availabilityStorage.GetFilteredResults(searchId: searchId, 
                filters: null, 
                searchSettings: searchSettings, 
                suppliers: searchSettings.EnabledConnectors,
                languageCode: "en");
            
            return new WideSearchResult(searchId, isComplete, result.MapFromEdoModels());
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
                        .Start(searchId, request.ToEdoModel(), supplierCodeMappings, supplier, agent, languageCode, searchSettings);
                });
            }
        }


        private async Task<bool> IsComplete(Guid searchId, AgentContext agent)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            var searchStates = await _stateStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            var state = WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);

            return state.TaskState is not AvailabilitySearchTaskState.Pending or AvailabilitySearchTaskState.PartiallyCompleted;
        }

        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IAvailabilitySearchAreaService _searchAreaService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
    }
}