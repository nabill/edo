using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public interface IBookingEvaluationPriceProcessor
    {
        Task<RoomContractSetAvailability> ApplyMarkups(RoomContractSetAvailability response, AgentContext agent, Action<MarkupApplicationResult<RoomContractSetAvailability>> logAction);

        Task<Result<RoomContractSetAvailability, ProblemDetails>> ConvertCurrencies(RoomContractSetAvailability availabilityDetails);

        Task<RoomContractSetAvailability> AlignPrices(RoomContractSetAvailability availabilityDetails);
    }
}