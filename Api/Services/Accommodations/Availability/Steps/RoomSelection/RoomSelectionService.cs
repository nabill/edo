using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IDateTimeProvider = HappyTravel.Edo.Api.Infrastructure.IDateTimeProvider;
using RoomContractSet = HappyTravel.Edo.Api.Models.Accommodations.RoomContractSet;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public class RoomSelectionService : IRoomSelectionService
    {
        public RoomSelectionService(IWideAvailabilityStorage wideAvailabilityStorage,
            IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IDateTimeProvider dateTimeProvider,
            IServiceScopeFactory serviceScopeFactory,
            IWideAvailabilitySearchStateStorage stateStorage,
            IBookingAnalyticsService bookingAnalyticsService,
            IAccommodationMapperClient mapperClient,
            ILogger<RoomSelectionService> logger)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _dateTimeProvider = dateTimeProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _bookingAnalyticsService = bookingAnalyticsService;
            _wideAvailabilityStorage = wideAvailabilityStorage;
            _mapperClient = mapperClient;
            _logger = logger;
            _stateStorage = stateStorage;
        }


        public async Task<Result<AvailabilitySearchTaskState>> GetState(Guid searchId, string htId, AgentContext agent)
        {
            var settings = await _accommodationBookingSettingsService.Get(agent);
            var results = await _stateStorage.GetStates(searchId, settings.EnabledConnectors);
            return WideAvailabilitySearchState.FromSupplierStates(searchId, results).TaskState;
        }


        public async Task<Result<AgentAccommodation, ProblemDetails>> GetAccommodation(Guid searchId, string htId, AgentContext agent, string languageCode)
        {
            Baggage.AddSearchId(searchId);

            var accommodation = await _mapperClient.GetAccommodation(htId, languageCode);
            if (accommodation.IsFailure)
                return accommodation.Error;

            _bookingAnalyticsService.LogAccommodationAvailabilityRequested(accommodation.Value, searchId, htId, agent);

            var searchSettings = await _accommodationBookingSettingsService.Get(agent);

            return accommodation.Value.ToEdoContract().ToAgentAccommodation(searchSettings.IsSupplierVisible);
        }


        public async Task<Result<List<RoomContractSet>>> Get(Guid searchId, string htId, AgentContext agent, string languageCode)
        {
            Baggage.AddSearchId(searchId);
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);

            var (_, isFailure, selectedResults, error) = await GetSelectedWideAvailabilityResults(searchId, htId, agent);
            if (isFailure)
                return Result.Failure<List<RoomContractSet>>(error);

            var checkInDate = selectedResults
                .Select(s => s.Result.CheckInDate)
                .FirstOrDefault();

            var supplierTasks = selectedResults
                .Select(GetSupplierAvailability)
                .ToArray();

            await Task.WhenAll(supplierTasks);

            return supplierTasks
                .Select(task => task.Result)
                .Where(taskResult => taskResult.IsSuccess)
                .Select(taskResult => taskResult.Value)
                .SelectMany(MapToRoomContractSets)
                .Where(SettingsFilter)
                .OrderBy(r => r.Rate.FinalPrice.Amount)
                .ToList();


            async Task<Result<SingleAccommodationAvailability, ProblemDetails>> GetSupplierAvailability((string, AccommodationAvailabilityResult) wideAvailabilityResult)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var (source, result) = wideAvailabilityResult;

                return await RoomSelectionSearchTask
                    .Create(scope.ServiceProvider)
                    .GetSupplierAvailability(searchId: searchId, htId: htId, supplierCode: source, supplierAccommodationCode: result.SupplierAccommodationCode,
                        availabilityId: result.AvailabilityId, settings: searchSettings, agent: agent, languageCode: languageCode,
                        countryHtId: result.CountryHtId, localityHtId: result.LocalityHtId, result.RegionId);
            }


            async Task<Result<List<(string Source, AccommodationAvailabilityResult Result)>>> GetSelectedWideAvailabilityResults(Guid searchId, string htId, AgentContext agent)
            {
                var results = await GetWideAvailabilityResults(searchId, htId, agent);
                if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                {
                    results = results
                        .Where(r => r.Result.CheckInDate > _dateTimeProvider.UtcTomorrow());
                }

                return results.ToList();
            }


            IEnumerable<RoomContractSet> MapToRoomContractSets(SingleAccommodationAvailability accommodationAvailability)
            {
                return accommodationAvailability.RoomContractSets
                    .Select(rs => rs.ApplySearchSettings(searchSettings.IsSupplierVisible, searchSettings.IsDirectContractFlagVisible));
            }


            bool SettingsFilter(RoomContractSet roomSet)
                => RoomContractSetSettingsChecker.IsDisplayAllowed(roomSet, checkInDate, searchSettings, _dateTimeProvider);
        }


        private async Task<IEnumerable<(string SupplierCode, AccommodationAvailabilityResult Result)>> GetWideAvailabilityResults(Guid searchId, string htId,
            AgentContext agent)
        {
            var settings = await _accommodationBookingSettingsService.Get(agent);
            return (await _wideAvailabilityStorage.GetResults(searchId, settings.EnabledConnectors))
                .SelectMany(r => r.AccommodationAvailabilities.Select(acr => (Source: r.SupplierCode, Result: acr)))
                .Where(r => r.Result.HtId == htId);
        }


        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBookingAnalyticsService _bookingAnalyticsService;
        private readonly IWideAvailabilityStorage _wideAvailabilityStorage;
        private readonly IAccommodationMapperClient _mapperClient;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly ILogger<RoomSelectionService> _logger;
    }
}