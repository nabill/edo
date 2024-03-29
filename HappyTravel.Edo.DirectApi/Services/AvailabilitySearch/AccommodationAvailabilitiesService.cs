﻿using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    public class AccommodationAvailabilitiesService
    {
        public AccommodationAvailabilitiesService(IAccommodationBookingSettingsService accommodationBookingSettingsService, 
            IWideAvailabilitySearchStateStorage stateStorage, IRoomSelectionService roomSelectionService)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _stateStorage = stateStorage;
            _roomSelectionService = roomSelectionService;
        }


        public async Task<Result<RoomSelectionResult>> Get(Guid searchId, string htId)
        {
            while (!await IsComplete(searchId))
                await Task.Delay(TimeSpan.FromSeconds(1));
            
            var (_, isFailure, result, error) = await _roomSelectionService.Get(searchId, htId, "en");

            return isFailure 
                ? Result.Failure<RoomSelectionResult>(error) 
                : new RoomSelectionResult(searchId, htId, result.MapFromEdoModels());
        }
        
        
        private async Task<bool> IsComplete(Guid searchId)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get();
            var searchStates = await _stateStorage.GetStates(searchId, searchSettings.EnabledConnectors);
            var state = WideAvailabilitySearchState.FromSupplierStates(searchId, searchStates);

            return state.TaskState is not AvailabilitySearchTaskState.Pending or AvailabilitySearchTaskState.PartiallyCompleted;
        }
        
        
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly IRoomSelectionService _roomSelectionService;
    }
}