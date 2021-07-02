using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.SuppliersCatalog;
using RoomContractSetAvailability = HappyTravel.EdoContracts.Accommodations.RoomContractSetAvailability;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public interface IBookingEvaluationStorage
    {
        Task Set(Guid searchId, Guid roomContractSetId, DataWithMarkup<RoomContractSetAvailability> availability, Suppliers resultSupplier,
            List<PaymentTypes> availablePaymentTypes, string htId, SlimAccommodation accommodation, Deadline supplierDeadline);
        
        Task<Result<BookingAvailabilityInfo>> Get(Guid searchId, string htId, Guid roomContractSetId);
    }
}