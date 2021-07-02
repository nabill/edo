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
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.SuppliersCatalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
            AvailabilityAnalyticsService analyticsService,
            IAccommodationMapperClient mapperClient)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _dateTimeProvider = dateTimeProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _analyticsService = analyticsService;
            _wideAvailabilityStorage = wideAvailabilityStorage;
            _mapperClient = mapperClient;
        }


        public async Task<Result<AvailabilitySearchTaskState>> GetState(Guid searchId, string htId, AgentContext agent)
        {
            var settings = await _accommodationBookingSettingsService.Get(agent);
            var results = await _wideAvailabilityStorage.GetStates(searchId, settings.EnabledConnectors);
            return WideAvailabilitySearchState.FromSupplierStates(searchId, results).TaskState;
        }
        
        
        public async Task<Result<Accommodation, ProblemDetails>> GetAccommodation(Guid searchId, string htId, AgentContext agent, string languageCode)
        {
            Baggage.SetSearchId(searchId);
            var (_, isFailure, selectedResult, error) = await GetSelectedResult(searchId, htId, agent);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<Accommodation>(error);

            _analyticsService.LogAccommodationAvailabilityRequested(selectedResult.Result, searchId, htId, agent);

            var accommodation = await _mapperClient.GetAccommodation(selectedResult.Result.Accommodation.HtId, languageCode);
            return accommodation.IsFailure
                ? accommodation.Error
                : accommodation.Value.ToEdoContract();
        }


        public async Task<Result<List<RoomContractSet>>> Get(Guid searchId, string htId, AgentContext agent, string languageCode)
        {
            Baggage.SetSearchId(searchId);
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
                .ToList();


            async Task<Result<SupplierData<AccommodationAvailability>, ProblemDetails>> GetSupplierAvailability((Suppliers, AccommodationAvailabilityResult) wideAvailabilityResult)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var (source, result) = wideAvailabilityResult;

                return await RoomSelectionSearchTask
                    .Create(scope.ServiceProvider)
                    .GetSupplierAvailability(searchId, htId, source, result.Accommodation, result.AvailabilityId, searchSettings, agent, languageCode);
            }
            

            async Task<Result<List<(Suppliers Source, AccommodationAvailabilityResult Result)>>> GetSelectedWideAvailabilityResults(Guid searchId, string htId, AgentContext agent)
            {
                var results = await GetWideAvailabilityResults(searchId, agent);
                
                var selectedResult = results
                    .SingleOrDefault(r => r.Result.HtId == htId);

                if (selectedResult.Equals(default))
                    return Result.Failure<List<(Suppliers, AccommodationAvailabilityResult)>>("Could not find selected availability");

                if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide &&
                    selectedResult.Result.CheckInDate.Date <= _dateTimeProvider.UtcTomorrow())
                {
                    return Result.Failure<List<(Suppliers, AccommodationAvailabilityResult)>>("You can't book the contract within deadline without explicit approval from a Happytravel.com officer.");
                }

                // If there is no duplicate, we'll execute request to a single supplier only
                if (string.IsNullOrWhiteSpace(selectedResult.Result.HtId))
                    return new List<(Suppliers Source, AccommodationAvailabilityResult Result)> {selectedResult};

                return results
                    .Where(r => r.Result.HtId == selectedResult.Result.HtId)
                    .ToList();
            }

            
            IEnumerable<RoomContractSet> MapToRoomContractSets(SupplierData<AccommodationAvailability> accommodationAvailability)
            {
                return accommodationAvailability.Data.RoomContractSets
                    .Select(rs =>
                    {
                        var supplier = searchSettings.IsSupplierVisible
                            ? accommodationAvailability.Source
                            : (Suppliers?) null;

                        var isDirectContractFlag = searchSettings.IsDirectContractFlagVisible && rs.IsDirectContract;

                        return rs.ToRoomContractSet(supplier, isDirectContractFlag);
                    });
            }


            bool SettingsFilter(RoomContractSet roomSet) 
                => RoomContractSetSettingsChecker.IsDisplayAllowed(roomSet, checkInDate, searchSettings, _dateTimeProvider);
        }


        private async Task<List<(Suppliers Supplier, AccommodationAvailabilityResult Result)>> GetWideAvailabilityResults(Guid searchId, AgentContext agent)
        {
            var settings = await _accommodationBookingSettingsService.Get(agent);
            return (await _wideAvailabilityStorage.GetResults(searchId, settings.EnabledConnectors))
                .SelectMany(r => r.AccommodationAvailabilities.Select(acr => (Source: r.SupplierKey, Result: acr)))
                .ToList();
        }


        private async Task<Result<(Suppliers Supplier, AccommodationAvailabilityResult Result)>> GetSelectedResult(Guid searchId, string htId, AgentContext agent)
        {
            var result = (await GetWideAvailabilityResults(searchId, agent))
                .SingleOrDefault(r => r.Result.HtId == htId);

            return result.Equals(default)
                ? Result.Failure<(Suppliers, AccommodationAvailabilityResult)>("Could not find selected availability")
                : result;
        }
        
        
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AvailabilityAnalyticsService _analyticsService;
        private readonly IWideAvailabilityStorage _wideAvailabilityStorage;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}