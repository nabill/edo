using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using RoomContractSetAvailability = HappyTravel.EdoContracts.Accommodations.RoomContractSetAvailability;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationStorage : IBookingEvaluationStorage
    {
        public BookingEvaluationStorage(IDoubleFlow doubleFlow)
        {
            _doubleFlow = doubleFlow;
        }


        public Task Set(Guid searchId, Guid resultId, Guid roomContractSetId, DataWithMarkup<RoomContractSetAvailability> availability,
            Suppliers supplier)
        {
            var key = BuildKey(searchId, resultId, roomContractSetId);
            var dataToSave = SupplierData.Create(supplier, availability);
            return _doubleFlow.SetAsync(key, dataToSave, CacheExpirationTime);
        }


        public async Task<Result<(Suppliers Source, DataWithMarkup<RoomContractSetAvailability> Result)>> Get(Guid searchId, Guid resultId, Guid roomContractSetId, List<Suppliers> suppliers)
        {
            var key = BuildKey(searchId, resultId, roomContractSetId);
            
            var result = await _doubleFlow.GetAsync<SupplierData<DataWithMarkup<RoomContractSetAvailability>>>(key, CacheExpirationTime);
            return result.Equals(default)
                ? Result.Failure<(Suppliers, DataWithMarkup<RoomContractSetAvailability>)>("Could not find evaluation result")
                : (result.Source, result.Data);
        }

        
        private string BuildKey(Guid searchId, Guid resultId, Guid roomContractSetId) => $"{searchId}::{resultId}::{roomContractSetId}";
        
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);
        private readonly IDoubleFlow _doubleFlow;
    }
}