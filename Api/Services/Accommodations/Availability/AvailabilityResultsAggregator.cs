using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityResultsAggregator 
    {
        public AvailabilityResultsAggregator(AvailabilityStorage storage,
            IServiceProvider serviceProvider)
        {
            _storage = storage;
            _serviceProvider = serviceProvider;
        }


        public Guid StartSearch(AvailabilityRequest availabilityRequest, AgentInfo agent, string languageCode)
        {
            var availabilityService = _serviceProvider.GetRequiredService<IAvailabilityService>();
            var searchId = Guid.NewGuid();
            _storage.SaveState(searchId, new AvailabilitySearchState(AvailabilitySearchTaskState.Running));

            Task.Run(async () =>
            {
                try
                {
                    var (isSuccess, _, result, error) = await availabilityService.GetAvailable(availabilityRequest, agent, languageCode);
                    if (isSuccess)
                        await _storage.SaveResult(searchId, result);
            
                    var state = isSuccess
                        ? new AvailabilitySearchState(AvailabilitySearchTaskState.Ready, result.Results.Count)
                        : new AvailabilitySearchState(AvailabilitySearchTaskState.Failed, error: error.Detail);

                    await _storage.SaveState(searchId, state);
                }
                catch (Exception e)
                {
                    await _storage.SaveState(searchId, new AvailabilitySearchState(AvailabilitySearchTaskState.Failed, error: e.Message));
                }
            });
            
            return searchId;
        }


        public Task<AvailabilitySearchState> GetState(Guid ticketId) => _storage.GetState(ticketId);


        public async ValueTask<Result<CombinedAvailabilityDetails>> GetResult(Guid ticketId)
        {
            var state = await _storage.GetState(ticketId);
            switch (state.TaskState)
            {
                case AvailabilitySearchTaskState.Ready:
                    return Result.Ok(await _storage.GetResult(ticketId));
                default:
                    return Result.Fail<CombinedAvailabilityDetails>("Task has not completed yet");
            }
        }


        private readonly AvailabilityStorage _storage;
        private readonly IServiceProvider _serviceProvider;
    }
}