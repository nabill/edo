using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationService : IBookingEvaluationService
    {
        public BookingEvaluationService(IDataProviderManager dataProviderManager,
            IPriceProcessor priceProcessor,
            IRoomSelectionStorage roomSelectionStorage,
            IBookingEvaluationStorage bookingEvaluationStorage)
        {
            _dataProviderManager = dataProviderManager;
            _priceProcessor = priceProcessor;
            _roomSelectionStorage = roomSelectionStorage;
            _bookingEvaluationStorage = bookingEvaluationStorage;
        }
        
        public async Task<Result<RoomContractSetAvailability?, ProblemDetails>> GetExactAvailability(
            Guid searchId, Guid resultId, Guid roomContractSetId, AgentContext agent, string languageCode)
        {
            var (_, isFailure, result, error) = await GetSelectedRoomSet(searchId, resultId, roomContractSetId);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<RoomContractSetAvailability?>(error);
            
            return await EvaluateOnConnector(result)
                .Bind(ConvertCurrencies)
                .Map(ApplyMarkups)
                .Tap(SaveToCache)
                .Map(ToDetails);


            async Task<Result<(DataProviders DataProvider, RoomContractSet, string)>> GetSelectedRoomSet(Guid searchId, Guid resultId, Guid roomContractSetId)
            {
                var result = (await _roomSelectionStorage.GetResult(searchId, resultId, await _dataProviderManager.GetEnabled(agent)))
                    .SelectMany(r =>
                    {
                        return r.Result.RoomContractSets
                            .Select(rs => (Source: r.DataProvider, RoomContractSet: rs, AvailabilityId: r.Result.AvailabilityId));
                    })
                    .SingleOrDefault(r => r.RoomContractSet.Id == roomContractSetId);

                if (result.Equals(default))
                    return Result.Failure<(DataProviders, RoomContractSet, string)>("Could not find selected room contract set");

                return result;
            }

            
            Task<Result<RoomContractSetAvailability?, ProblemDetails>> EvaluateOnConnector((DataProviders, RoomContractSet, string) selectedSet)
            {
                var (provider, roomContractSet, availabilityId) = selectedSet;
                return _dataProviderManager
                    .Get(provider)
                    .GetExactAvailability(availabilityId, roomContractSet.Id, languageCode);
            }


            Task<Result<RoomContractSetAvailability?, ProblemDetails>> ConvertCurrencies(RoomContractSetAvailability? availabilityDetails) => _priceProcessor.ConvertCurrencies(agent,
                availabilityDetails,
                AvailabilityResultsExtensions.ProcessPrices,
                AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<RoomContractSetAvailability?>>
                ApplyMarkups(RoomContractSetAvailability? response)
                => _priceProcessor.ApplyMarkups(agent, response, AvailabilityResultsExtensions.ProcessPrices);


            Task SaveToCache(DataWithMarkup<RoomContractSetAvailability?> responseWithDeadline)
            {
                if(!responseWithDeadline.Data.HasValue)
                    return Task.CompletedTask;

                // TODO: Check that this id will not change on all connectors NIJO-823
                var finalRoomContractSetId = responseWithDeadline.Data.Value.RoomContractSet.Id;
                return _bookingEvaluationStorage.Set(searchId, resultId, finalRoomContractSetId, DataWithMarkup.Create(responseWithDeadline.Data.Value, 
                    responseWithDeadline.Policies), result.DataProvider);
            }


            RoomContractSetAvailability? ToDetails(
                DataWithMarkup<RoomContractSetAvailability?> availabilityDetails)
                => availabilityDetails.Data;
        }
        
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IPriceProcessor _priceProcessor;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
    }
}