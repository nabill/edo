using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.Step1
{
    public interface IAvailabilitySearchScheduler
    {
        Task<Result<Guid>> StartSearch(AvailabilityRequest request, List<DataProviders> dataProviders, AgentContext agent, string languageCode);
    }
}