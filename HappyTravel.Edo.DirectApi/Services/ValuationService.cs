using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class ValuationService
    {
        public ValuationService(IBookingEvaluationService bookingEvaluationService)
        {
            _bookingEvaluationService = bookingEvaluationService;
        }
        
        
        public async Task<Result<RoomContractSetAvailability>> Get(Guid searchId, string htId, Guid roomContractSetId, AgentContext agent, string languageCode)
        {
            var (_, isFailure, availability, error) = await _bookingEvaluationService.GetExactAvailability(searchId, htId, roomContractSetId, agent, languageCode);

            return isFailure
                ? Result.Failure<RoomContractSetAvailability>(error.Detail)
                : availability.Value.MapFromEdoModels();
        }


        private readonly IBookingEvaluationService _bookingEvaluationService;
    }
}