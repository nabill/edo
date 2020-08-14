using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationResultsStorage : IBookingEvaluationResultsStorage
    {
        public BookingEvaluationResultsStorage(IAvailabilityStorage availabilityStorage)
        {
            _availabilityStorage = availabilityStorage;
        }


        public Task Set(Guid searchId, Guid resultId, Guid roomContractSetId, DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline> availability,
            DataProviders dataProvider)
        {
            var key = CreateKeyPrefix(searchId, resultId, roomContractSetId);
            return _availabilityStorage.SaveObject(key, availability, dataProvider);
        }


        public Task<Result<(DataProviders Source, DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline> Result)>> Get(Guid searchId, Guid resultId, Guid roomContractSetId, List<DataProviders> dataProviders) => throw new NotImplementedException();


        private string CreateKeyPrefix(Guid searchId, Guid resultId, Guid roomContractSetId) => $"{searchId}::{resultId}::{roomContractSetId}";

        private readonly IAvailabilityStorage _availabilityStorage;
    }
}