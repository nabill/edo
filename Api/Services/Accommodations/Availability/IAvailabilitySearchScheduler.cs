using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IAvailabilitySearchScheduler
    {
        Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentInfo agent, string languageCode);
    }
}