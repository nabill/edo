using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    public class ValuationService
    {
        public ValuationService(IBookingEvaluationService bookingEvaluationService)
        {
            _bookingEvaluationService = bookingEvaluationService;
        }
        
        
        public async Task<Result<RoomContractSetAvailability>> Get(Guid searchId, string accommodationId, Guid roomContractSetId, AgentContext agent, string languageCode)
        {
            var (_, isFailure, availability, error) = await _bookingEvaluationService.GetExactAvailability(searchId, accommodationId, roomContractSetId, agent, languageCode);

            return isFailure
                ? Result.Failure<RoomContractSetAvailability>(error.Detail)
                : availability?.MapFromEdoModels(searchId, accommodationId) 
                ?? new RoomContractSetAvailability(searchId, accommodationId, string.Empty, null);
        }


        private readonly IBookingEvaluationService _bookingEvaluationService;
    }
}