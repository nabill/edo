using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public interface IBookingEvaluationStorage
    {
        Task<Result> Set(Guid searchId, Guid roomContractSetId, string htId, DataWithMarkup<RoomContractSetAvailability> availability, Deadline agentDeadline,
            Deadline supplierDeadline, CreditCardRequirement? cardRequirement, string supplierAccommodationCode);
        
        Task<Result<BookingAvailabilityInfo>> Get(Guid searchId, string htId, Guid roomContractSetId);
    }
}