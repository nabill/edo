using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step1
{
    public interface IAvailabilitySearchScheduler
    {
        Task<Result<Guid>> StartSearch(AvailabilityRequest request, AgentContext agent, string languageCode);
    }
}