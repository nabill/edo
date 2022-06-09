using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using RoomContractSetAvailability = HappyTravel.Edo.Api.Models.Accommodations.RoomContractSetAvailability;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public interface IBookingEvaluationService
    {
        Task<Result<RoomContractSetAvailability?, ProblemDetails>> GetExactAvailability(Guid searchId, string htId, Guid roomContractSetId, string languageCode);
    }
}