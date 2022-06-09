using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public interface IRoomSelectionService
    {
        Task<Result<AvailabilitySearchTaskState>> GetState(Guid searchId);

        Task<Result<AgentAccommodation, ProblemDetails>> GetAccommodation(Guid searchId, string htId, string languageCode);

        Task<Result<List<RoomContractSet>>> Get(Guid searchId, string htId, string languageCode);
    }
}