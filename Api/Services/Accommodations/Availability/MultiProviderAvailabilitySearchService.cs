using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class MultiProviderAvailabilitySearchService 
    {
        public MultiProviderAvailabilitySearchService(IServiceScopeFactory serviceScopeFactory, 
            AvailabilityStorage storage,
            ILogger<MultiProviderAvailabilitySearchService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _storage = storage;
            _logger = logger;
        }


        public Guid StartSearch(AvailabilityRequest availabilityRequest, AgentInfo agent, string languageCode)
        {
            var searchId = Guid.NewGuid();
            _ = StartSearch(searchId, availabilityRequest, agent, languageCode);
            return searchId;
        }


        private async Task StartSearch(Guid searchId, AvailabilityRequest availabilityRequest, AgentInfo agent, string languageCode)
        {
            // This task usually finishes later than outer scope of this service is disposed.
            // Creating new scope helps to avoid early dependencies disposal
            // https://docs.microsoft.com/ru-ru/aspnet/core/performance/performance-best-practices?view=aspnetcore-3.1#do-not-capture-services-injected-into-the-controllers-on-background-threads
            using var serviceScope = _serviceScopeFactory.CreateScope();

            var availabilityService = serviceScope.ServiceProvider.GetRequiredService<IAvailabilityService>();
            var storage = serviceScope.ServiceProvider.GetRequiredService<AvailabilityStorage>();
            
            try
            {
                // TODO: Add delegate-based logging with event id
                _logger.LogInformation($"Starting availability search with id '{searchId}'");
                await storage.SaveState(searchId, new AvailabilitySearchState(searchId, AvailabilitySearchTaskState.Running));
                
                var (isSuccess, _, result, error) = await availabilityService
                    .GetAvailable(availabilityRequest, agent, languageCode);
                
                if (isSuccess)
                    await storage.SaveResult(searchId, result);

                var state = isSuccess
                    ? new AvailabilitySearchState(searchId, AvailabilitySearchTaskState.Ready, result.Results.Count)
                    : new AvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed, error: error.Detail);

                await storage.SaveState(searchId, state);
                // TODO: Add delegate-based logging with event id
                _logger.LogInformation($"Finished availability search with id '{searchId}'. Results count: {result.Results.Count}");
            }
            catch (Exception ex)
            {
                await storage.SaveState(searchId, new AvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed, error: ex.Message));
                // TODO: Add delegate-based logging with event id
                // TODO: Enrich exception with span and trace id
                _logger.LogError($"Availability search task failed with error: {ex.Message}");
                throw;
            }
        }


        public Task<AvailabilitySearchState> GetState(Guid searchId) => _storage.GetState(searchId);


        public async ValueTask<Result<CombinedAvailabilityDetails>> GetResult(Guid searchId)
        {
            var state = await _storage.GetState(searchId);
            switch (state.TaskState)
            {
                case AvailabilitySearchTaskState.Ready:
                    return Result.Ok(await _storage.GetResult(searchId));
                case AvailabilitySearchTaskState.Running:
                    return Result.Fail<CombinedAvailabilityDetails>("Task has not completed yet");
                case AvailabilitySearchTaskState.Failed:
                    return Result.Fail<CombinedAvailabilityDetails>($"Task has completed with error {state.Error}");
                default:
                    return Result.Fail<CombinedAvailabilityDetails>($"Invalid task state {state.TaskState}");
            }
        }
        
        
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AvailabilityStorage _storage;
        private readonly ILogger<MultiProviderAvailabilitySearchService> _logger;
    }
}