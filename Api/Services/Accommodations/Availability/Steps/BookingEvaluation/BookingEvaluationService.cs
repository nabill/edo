using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationService : IBookingEvaluationService
    {
        public BookingEvaluationService(ISupplierConnectorManager supplierConnectorManager,
            IPriceProcessor priceProcessor,
            IRoomSelectionStorage roomSelectionStorage,
            IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IDateTimeProvider dateTimeProvider,
            IBookingEvaluationStorage bookingEvaluationStorage)
        {
            _supplierConnectorManager = supplierConnectorManager;
            _priceProcessor = priceProcessor;
            _roomSelectionStorage = roomSelectionStorage;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _dateTimeProvider = dateTimeProvider;
            _bookingEvaluationStorage = bookingEvaluationStorage;
        }
        
        public async Task<Result<RoomContractSetAvailabilityInfo?, ProblemDetails>> GetExactAvailability(
            Guid searchId, Guid resultId, Guid roomContractSetId, AgentContext agent, string languageCode)
        {
            var settings = await _accommodationBookingSettingsService.Get(agent);
            var (_, isFailure, result, error) = await GetSelectedRoomSet(searchId, resultId, roomContractSetId);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<RoomContractSetAvailabilityInfo?>(error);
            
            return await EvaluateOnConnector(result)
                .Bind(CheckAgainstSettings)
                .Bind(ConvertCurrencies)
                .Map(ApplyMarkups)
                .Tap(SaveToCache)
                .Map(ToDetails);


            async Task<Result<(Suppliers Supplier, RoomContractSet, string)>> GetSelectedRoomSet(Guid searchId, Guid resultId, Guid roomContractSetId)
            {
                var result = (await _roomSelectionStorage.GetResult(searchId, resultId, settings.EnabledConnectors))
                    .SelectMany(r =>
                    {
                        return r.Result.RoomContractSets
                            .Select(rs => (Source: r.DataProvider, RoomContractSet: rs, AvailabilityId: r.Result.AvailabilityId));
                    })
                    .SingleOrDefault(r => r.RoomContractSet.Id == roomContractSetId);

                if (result.Equals(default))
                    return Result.Failure<(Suppliers, RoomContractSet, string)>("Could not find selected room contract set");
                
                return result;
            }

            
            Task<Result<RoomContractSetAvailability?, ProblemDetails>> EvaluateOnConnector((Suppliers, RoomContractSet, string) selectedSet)
            {
                var (provider, roomContractSet, availabilityId) = selectedSet;
                return _supplierConnectorManager
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
                    responseWithDeadline.Policies), result.Supplier);
            }


            RoomContractSetAvailabilityInfo? ToDetails(DataWithMarkup<RoomContractSetAvailability?> availabilityDetails)
            {
                var provider = settings.IsDataProviderVisible
                    ? result.Supplier
                    : (Suppliers?) null;
                
                return RoomContractSetAvailabilityInfo.FromRoomContractSetAvailability(availabilityDetails.Data, provider);
            }


            Result<RoomContractSetAvailability?, ProblemDetails> CheckAgainstSettings(RoomContractSetAvailability? availability)
            {
                if (availability is null)
                    return (RoomContractSetAvailability?)null;

                if (settings.AprMode == AprMode.Hide || settings.AprMode == AprMode.DisplayOnly)
                {
                    if (availability.Value.RoomContractSet.IsAdvancedPurchaseRate)
                        return ProblemDetailsBuilder.Fail<RoomContractSetAvailability?>("You can't book the restricted contract without explicit approval from a Happytravel.com officer.");
                }
                    
                if (settings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide ||
                    settings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.DisplayOnly)
                {
                    var deadlineDate = availability.Value.RoomContractSet.Deadline.Date;
                    if(deadlineDate.HasValue && deadlineDate.Value.Date <= _dateTimeProvider.UtcTomorrow())
                        return ProblemDetailsBuilder.Fail<RoomContractSetAvailability?>("You can't book the contract within deadline without explicit approval from a Happytravel.com officer.");
                }

                return availability;
            }
        }
        
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IPriceProcessor _priceProcessor;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
    }
}