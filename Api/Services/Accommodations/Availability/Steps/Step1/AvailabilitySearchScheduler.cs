using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step1
{
    public class AvailabilitySearchScheduler : IAvailabilitySearchScheduler
    {
        public AvailabilitySearchScheduler(IServiceScopeFactory serviceScopeFactory,
            IDataProviderFactory dataProviderFactory,
            ILocationService locationService,
            IDateTimeProvider dateTimeProvider,
            IAccommodationDuplicatesService duplicatesService,
            ILogger<AvailabilitySearchScheduler> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _dataProviderFactory = dataProviderFactory;
            _locationService = locationService;
            _dateTimeProvider = dateTimeProvider;
            _duplicatesService = duplicatesService;
            _logger = logger;
        }


        public async Task<Result<Guid>> StartSearch(AvailabilityRequest request, List<DataProviders> dataProviders, AgentContext agent, string languageCode)
        {
            var searchId = Guid.NewGuid();
            _logger.LogMultiProviderAvailabilitySearchStarted($"Starting availability search with id '{searchId}'");
            
            var (_, isFailure, location, locationError) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Failure<Guid>(locationError.Detail);

            StartSearchTasks(searchId, request, dataProviders, location, agent, languageCode);
            
            return Result.Ok(searchId);
        }


        private void StartSearchTasks(Guid searchId, AvailabilityRequest request, List<DataProviders> requestedProviders, Location location, AgentContext agent, string languageCode)
        {
            var contractsRequest = ConvertRequest(request, location);

            foreach (var provider in GetProvidersToSearch(location, requestedProviders))
            {
                Task.Run(() => StartProviderSearch(searchId,
                    contractsRequest,
                    agent,
                    languageCode,
                    provider.Key,
                    provider.Provider));
            }


            IReadOnlyCollection<(DataProviders Key, IDataProvider Provider)> GetProvidersToSearch(in Location location, List<DataProviders> dataProviders)
            {
                var providers = location.DataProviders != null && location.DataProviders.Any()
                    ? location.DataProviders.Intersect(dataProviders).ToList()
                    : dataProviders;

                return _dataProviderFactory.Get(providers);
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


        private async Task StartProviderSearch(Guid searchId, EdoContracts.Accommodations.AvailabilityRequest request, AgentContext agent, string languageCode,
            DataProviders providerKey, IDataProvider dataProvider)
        {
            // This task usually finishes later than outer scope of this service is disposed.
            // Creating new scope helps to avoid early dependencies disposal
            // https://docs.microsoft.com/ru-ru/aspnet/core/performance/performance-best-practices?view=aspnetcore-3.1#do-not-capture-services-injected-into-the-controllers-on-background-threads
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var storage = serviceScope.ServiceProvider.GetRequiredService<IAvailabilityStorage>();
            var priceProcessor = serviceScope.ServiceProvider.GetRequiredService<IPriceProcessor>();

            try
            {
                _logger.LogProviderAvailabilitySearchStarted($"Availability search with id '{searchId}' on provider '{providerKey}' started");

                await GetAvailability(request, languageCode)
                    .Bind(ConvertCurrencies)
                    .Map(ApplyMarkups)
                    .Tap(SaveResults)
                    .Finally(SaveState);
            }
            catch (Exception ex)
            {
                // TODO: Add sentry error notification
                _logger.LogProviderAvailabilitySearchException(ex);
                var result = ProblemDetailsBuilder.Fail<AvailabilityDetails>("Server error", HttpStatusCode.InternalServerError);
                await SaveState(result);
            }
            

            async Task<Result<AvailabilityDetails, ProblemDetails>> GetAvailability(EdoContracts.Accommodations.AvailabilityRequest request,
                string languageCode)
            {
                var saveToStorageTask = storage.SaveObject(searchId, ProviderAvailabilitySearchState.Pending(searchId), providerKey);
                var getAvailabilityTask = dataProvider.GetAvailability(request, languageCode);
                await Task.WhenAll(saveToStorageTask, getAvailabilityTask);

                return getAvailabilityTask.Result;
            }


            Task<Result<AvailabilityDetails, ProblemDetails>> ConvertCurrencies(AvailabilityDetails availabilityDetails)
                => priceProcessor.ConvertCurrencies(agent, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices,
                    AvailabilityResultsExtensions.GetCurrency);


            async Task<AvailabilityDetails> ApplyMarkups(AvailabilityDetails response)
            {
                var markup = await priceProcessor.ApplyMarkups(agent, response, AvailabilityResultsExtensions.ProcessPrices);
                return markup.Data;
            }


            async Task SaveResults(AvailabilityDetails details)
            {
                var providerAccommodationIds = details.Results
                    .Select(r=> new ProviderAccommodationId(providerKey, r.AccommodationDetails.Id))
                    .ToList();
            
                var duplicates = await _duplicatesService.GetDuplicateReports(providerAccommodationIds); 
                
                var timestamp = _dateTimeProvider.UtcNow().Ticks;
                var availabilityResults = details
                    .Results
                    .Select(accommodationAvailability =>
                    {
                        var minPrice = accommodationAvailability.RoomContractSets.Min(r => r.Price.NetTotal);
                        var maxPrice = accommodationAvailability.RoomContractSets.Max(r => r.Price.NetTotal);
                        var accommodationId = new ProviderAccommodationId(providerKey, accommodationAvailability.AccommodationDetails.Id);
                        var resultId = Guid.NewGuid();
                        var duplicateReportId = duplicates.TryGetValue(accommodationId, out var reportId)
                            ? reportId
                            : string.Empty;
                                
                        var result = new AccommodationAvailabilityResult(resultId,
                            timestamp,
                            details.AvailabilityId,
                            accommodationAvailability.AccommodationDetails,
                            accommodationAvailability.RoomContractSets,
                            duplicateReportId,
                            minPrice,
                            maxPrice);

                        return ProviderData.Create(providerKey, result);
                    })
                    .ToList();
                
                await storage.SaveObject(searchId, availabilityResults, providerKey);
            }


            Task SaveState(Result<AvailabilityDetails, ProblemDetails> result)
            {
                var state = result.IsSuccess
                    ? ProviderAvailabilitySearchState.Completed(searchId, result.Value.Results.Count)
                    : ProviderAvailabilitySearchState.Failed(searchId, result.Error.Detail);

                if (state.TaskState == AvailabilitySearchTaskState.Completed)
                {
                    _logger.LogProviderAvailabilitySearchSuccess($"Availability search with id '{searchId}' on provider '{providerKey}' finished successfully with '{state.ResultCount}' results");
                }
                else
                {
                    _logger.LogProviderAvailabilitySearchFailure($"Availability search with id '{searchId}' on provider '{providerKey}' finished with state '{state.TaskState}', error '{state.Error}'");
                }
                

                return storage.SaveObject(searchId, state, providerKey);
            }
        }


        private readonly IDataProviderFactory _dataProviderFactory;
        private readonly ILocationService _locationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccommodationDuplicatesService _duplicatesService;
        private readonly ILogger<AvailabilitySearchScheduler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
    }
}