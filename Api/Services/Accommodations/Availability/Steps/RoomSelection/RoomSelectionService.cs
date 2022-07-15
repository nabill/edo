using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Errors;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            IAgentContextService agentContext, IAvailabilityRequestStorage requestStorage, IAvailabilitySearchAreaService searchAreaService,
            ISupplierOptionsStorage supplierOptionsStorage,
            IOptionsMonitor<SecondStepSettings> secondStepSettings)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _dateTimeProvider = dateTimeProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _bookingAnalyticsService = bookingAnalyticsService;
            _wideAvailabilityStorage = wideAvailabilityStorage;
            _mapperClient = mapperClient;
            _agentContext = agentContext;
            _requestStorage = requestStorage;
            _searchAreaService = searchAreaService;
            _supplierOptionsStorage = supplierOptionsStorage;
            _stateStorage = stateStorage;
            _secondStepSettings = secondStepSettings;
        }


        public async Task<Result<AvailabilitySearchTaskState>> GetState(Guid searchId)
        {
            var settings = await _accommodationBookingSettingsService.Get();
            var results = await _stateStorage.GetStates(searchId, settings.EnabledConnectors);
            return WideAvailabilitySearchState.FromSupplierStates(searchId, results).TaskState;
        }


        public async Task<Result<AgentAccommodation, ProblemDetails>> GetAccommodation(Guid searchId, string htId, string languageCode)
        {
            Baggages.AddSearchId(searchId);

            var agent = await _agentContext.GetAgent();
            var accommodation = await _mapperClient.GetAccommodation(htId, languageCode);
            if (accommodation.IsFailure)
                return accommodation.Error;

            _bookingAnalyticsService.LogAccommodationAvailabilityRequested(accommodation.Value, searchId, htId, agent);

            var searchSettings = await _accommodationBookingSettingsService.Get();

            return accommodation.Value.ToEdoContract().ToAgentAccommodation(searchSettings.IsSupplierVisible);
        }


        public async Task<Result<List<RoomContractSet>>> Get(Guid searchId, string htId, string languageCode)
        {
            Baggages.AddSearchId(searchId);
            var agent = await _agentContext.GetAgent();
            var searchSettings = await _accommodationBookingSettingsService.Get();

            var (_, isFailure, selectedResults, error) = await GetSelectedWideAvailabilityResults(searchId, htId);
            if (isFailure)
                return Result.Failure<List<RoomContractSet>>(error);

            var checkInDate = selectedResults
                .Select(s => s.Result.CheckInDate)
                .FirstOrDefault();

            var supplierTasks = selectedResults
                .Select(GetSupplierAvailability)
                .ToArray();

            await Task.WhenAll(supplierTasks);

            var successfulTasks = supplierTasks.Where(t => t.Result.IsSuccess);
            var failedSuppliers = GetFailedSuppliers();

            if (_secondStepSettings.CurrentValue.RestartFirstStepIfCacheExpired && failedSuppliers.Any())
            {
                await RestartWideAvailabilitySearch(searchId, htId, failedSuppliers);
                
                // need to get fresh selected results, because availabilityIds were changed on connectors
                var refreshedSelectedResults = await GetSelectedWideAvailabilityResults(searchId, htId);
                if (refreshedSelectedResults.IsFailure)
                    return Result.Failure<List<RoomContractSet>>(refreshedSelectedResults.Error);

                var secondTryResults = refreshedSelectedResults.Value.Where(r => failedSuppliers.Contains(r.Source))
                    .Select(GetSupplierAvailability)
                    .ToArray();

                await Task.WhenAll(secondTryResults);

                foreach (var result in secondTryResults)
                {
                    if (result.Result.IsSuccess)
                        successfulTasks = successfulTasks.Append(result);
                }
            }
            

            return successfulTasks
                .Select(task => task.Result)
                .Where(taskResult => taskResult.IsSuccess)
                .Select(taskResult => taskResult.Value)
                .SelectMany(MapToRoomContractSets)
                .Where(SettingsFilter)
                .OrderBy(r => r.Rate.FinalPrice.Amount)
                .ToList();


            List<string> GetFailedSuppliers()
            {
                var failedSuppliers = new List<string>();
                for (var i = 0; i < supplierTasks.Length; i++)
                {
                    var result = supplierTasks[i].Result;
                    if (!result.IsFailure)
                        continue;

                    if (!result.Error.Extensions.TryGetSearchFailureCode(out var failureCode))
                        continue;

                    if (failureCode is SearchFailureCodes.AvailabilityNotFound)
                        failedSuppliers.Add(selectedResults[i].Source);
                }

                return failedSuppliers;
            }


            async Task<Result<SingleAccommodationAvailability, ProblemDetails>> GetSupplierAvailability(
                (string, AccommodationAvailabilityResult) wideAvailabilityResult)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var (source, result) = wideAvailabilityResult;

                return await RoomSelectionSearchTask
                    .Create(scope.ServiceProvider)
                    .GetSupplierAvailability(searchId: searchId, htId: htId, supplierCode: source, supplierAccommodationCode: result.SupplierAccommodationCode,
                        availabilityId: result.AvailabilityId, settings: searchSettings, agent: agent, languageCode: languageCode,
                        countryHtId: result.CountryHtId, localityHtId: result.LocalityHtId, result.MarketId, result.CountryCode);
            }


            async Task<Result<List<(string Source, AccommodationAvailabilityResult Result)>>> GetSelectedWideAvailabilityResults(Guid searchId, string htId)
            {
                var results = await GetWideAvailabilityResults(searchId, htId);
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


        private async Task<IEnumerable<(string SupplierCode, AccommodationAvailabilityResult Result)>> GetWideAvailabilityResults(Guid searchId, string htId)
        {
            var settings = await _accommodationBookingSettingsService.Get();
            return (await _wideAvailabilityStorage.GetResults(searchId, settings))
                .SelectMany(r => r.AccommodationAvailabilities.Select(acr => (Source: r.SupplierCode, Result: acr)))
                .Where(r => r.Result.HtId == htId);
        }


        private async Task RestartWideAvailabilitySearch(Guid searchId, string htId, List<string> failedSuppliers)
        {
            var agent = await _agentContext.GetAgent();
            var searchSettings = await _accommodationBookingSettingsService.Get();
            var languageCode = CultureInfo.CurrentCulture.Name;

            var request = (await _requestStorage.Get(searchId)).Value;
            var accommodationCodes = (await _searchAreaService.GetSearchArea(new List<string> { htId }, languageCode)).Value.AccommodationCodes;

            foreach (var supplierCode in failedSuppliers)
            {
                var (_, isFailure, supplier, _) = _supplierOptionsStorage.Get(supplierCode);
                if (isFailure || !accommodationCodes.TryGetValue(supplier.Code, out var supplierCodeMappings))
                {
                    continue;
                }

                // clear storage from failed data before starting a new search
                await _wideAvailabilityStorage.Clear(supplierCode, searchId);
                await StartSearchTask(supplier, supplierCodeMappings);
            }


            async Task StartSearchTask(SlimSupplier supplier, List<SupplierCodeMapping> supplierCodeMappings)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                await WideAvailabilitySearchTask
                    .Create(scope.ServiceProvider)
                    .Start(searchId, request, supplierCodeMappings, supplier, agent, languageCode, searchSettings, useCache: false);
            }
        }


        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBookingAnalyticsService _bookingAnalyticsService;
        private readonly IWideAvailabilityStorage _wideAvailabilityStorage;
        private readonly IAccommodationMapperClient _mapperClient;
        private readonly IWideAvailabilitySearchStateStorage _stateStorage;
        private readonly IAgentContextService _agentContext;
        private readonly IAvailabilityRequestStorage _requestStorage;
        private readonly IAvailabilitySearchAreaService _searchAreaService;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        private readonly IOptionsMonitor<SecondStepSettings> _secondStepSettings;
    }
}