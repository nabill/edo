using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public interface IRoomSelectionService
    {
        Task<Result<AvailabilitySearchTaskState>> GetState(Guid searchId, string htId, AgentContext agent);

        Task<Result<Accommodation, ProblemDetails>> GetAccommodation(Guid searchId, string htId, AgentContext agent, string languageCode);

        Task<Result<List<RoomContractSet>>> Get(Guid searchId, string htId, AgentContext agent, string languageCode);
    }
}