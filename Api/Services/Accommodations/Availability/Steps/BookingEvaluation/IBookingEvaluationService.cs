using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public interface IBookingEvaluationService
    {
        Task<Result<RoomContractSetAvailabilityInfo?, ProblemDetails>> GetExactAvailability(
            Guid searchId, Guid resultId, Guid roomContractSetId, AgentContext agent, string languageCode);
    }
}