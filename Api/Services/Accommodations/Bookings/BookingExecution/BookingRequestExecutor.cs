using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Edo.CreditCards.Services;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.Errors;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public class BookingRequestExecutor : IBookingRequestExecutor
    {
        public BookingRequestExecutor(ISupplierConnectorManager supplierConnectorManager,
            IBookingResponseProcessor responseProcessor,
            IBookingAnalyticsService bookingAnalyticsService,
            IBookingRecordsUpdater bookingRecordsUpdater,
            IDateTimeProvider dateTimeProvider,
            IBookingRequestStorage requestStorage,
            ICreditCardProvider creditCardProvider,
            ILogger<BookingRequestExecutor> logger)
        {
            _supplierConnectorManager = supplierConnectorManager;
            _responseProcessor = responseProcessor;
            _bookingAnalyticsService = bookingAnalyticsService;
            _bookingRecordsUpdater = bookingRecordsUpdater;
            _dateTimeProvider = dateTimeProvider;
            _requestStorage = requestStorage;
            _creditCardProvider = creditCardProvider;
            _logger = logger;
        }


        public async Task<Result<Booking>> Execute(Data.Bookings.Booking booking, AgentContext agent, string languageCode)
        {
            Baggage.AddBookingReferenceCode(booking.ReferenceCode);
            
            var requestInfoResult = await _requestStorage.Get(booking.ReferenceCode);
            if (requestInfoResult.IsFailure)
                return Result.Failure<Booking>(requestInfoResult.Error);

            var (bookingRequest, availabilityInfo) = requestInfoResult.Value;
            var creditCardResult = await GetCreditCard(booking, availabilityInfo);
            if (creditCardResult.IsFailure)
                return Result.Failure<Booking>(creditCardResult.Error);
                    
            var bookingRequestResult = await SendSupplierRequest(bookingRequest, availabilityInfo.AvailabilityId, booking, creditCardResult.Value, languageCode);
            if (bookingRequestResult.IsSuccess)
                _bookingAnalyticsService.LogBookingOccured(bookingRequest, booking, agent);
            
            await ProcessRequestResult(bookingRequestResult);
            return bookingRequestResult;
            
            async Task<Result<Booking>> SendSupplierRequest(AccommodationBookingRequest bookingRequest, string availabilityId,
                Data.Bookings.Booking booking, CreditCardInfo creditCard, string languageCode)
            {
                var features = new List<Feature>();

                var roomDetails = bookingRequest.RoomDetails
                    .Select(d => new SlimRoomOccupation(d.Type, d.Passengers, string.Empty, d.IsExtraBedNeeded))
                    .ToList();

                var creditCardInfo = creditCard is not null
                    ? new CreditCard(creditCard.Number, creditCard.ExpiryDate, creditCard.HolderName, creditCard.SecurityCode, CardVendor.AmericanExpress)
                    : (CreditCard?)null;

                var innerRequest = new BookingRequest(availabilityId: availabilityId,
                    roomContractSetId: bookingRequest.RoomContractSetId,
                    referenceCode: booking.ReferenceCode,
                    rooms: roomDetails,
                    features: features,
                    creditCard: creditCardInfo,
                    rejectIfUnavailable: bookingRequest.RejectIfUnavailable);

                try
                {
                    var (isSuccess, _, bookingResult, error) = await TimeObserver.Execute(observedFunc: () => _supplierConnectorManager
                        .Get((Suppliers) booking.Supplier)
                        .Book(innerRequest, languageCode),
                        notifyFunc: Notify,
                        notifyAfter: TimeSpan.FromSeconds(BookExecutionTimeLimitInSeconds));

                    if (isSuccess)
                        return bookingResult;

                    var message = error.Detail;
                    // If result is failed this does not mean that booking failed. All known cases are listed below
                    _logger.LogBookingFinalizationFailure(booking.ReferenceCode, message);

                    if (!error.Extensions.TryGetBookingFailureCode(out var failureCode))
                        // We do not know whether booking was registered on supplier
                        return GetStubDetails(booking);

                    return failureCode switch
                    {
                        // We are sure that booking was not done
                        BookingFailureCodes.ConnectorValidationFailed => Result.Failure<Booking>(message),
                        BookingFailureCodes.ValuationResultNotFound => Result.Failure<Booking>(message),
                        BookingFailureCodes.PreBookingFailed => Result.Failure<Booking>(message),
                        BookingFailureCodes.SupplierValidationFailed => Result.Failure<Booking>(message),
                        BookingFailureCodes.SupplierRejected => Result.Failure<Booking>(message),
                        
                        // We do not know whether booking was registered on supplier
                        _ => GetStubDetails(booking)
                    };
                    
                    
                    Task Notify()
                    {
                        _logger.LogBookingExceededTimeLimit(innerRequest.ReferenceCode);
                        return Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogBookingFinalizationException(ex);
                    return GetStubDetails(booking);
                }


                static EdoContracts.Accommodations.Booking GetStubDetails(Data.Bookings.Booking booking)
                    => new EdoContracts.Accommodations.Booking(booking.ReferenceCode,
                        // Will be set in the refresh step
                        BookingStatusCodes.WaitingForResponse,
                        booking.AccommodationId,
                        booking.SupplierReferenceCode,
                        booking.CheckInDate.DateTime,
                        booking.CheckOutDate.DateTime,
                        new List<SlimRoomOccupation>(0),
                        booking.UpdateMode);
            }
            
            
            async Task ProcessRequestResult(Result<Booking> responseResult)
            {
                if (responseResult.IsSuccess)
                {
                    await _responseProcessor.ProcessResponse(responseResult.Value, agent.ToApiCaller(), BookingChangeEvents.BookingRequest);
                }
                else
                {
                    var changeReason = new Data.Bookings.BookingChangeReason
                    {
                        Source = BookingChangeSources.System,
                        Event = BookingChangeEvents.BookingRequest
                    };
                    
                    await _bookingRecordsUpdater.ChangeStatus(booking, BookingStatuses.Invalid, _dateTimeProvider.UtcNow(), 
                        ApiCaller.InternalServiceAccount, changeReason);
                }
            }
        }


        private async ValueTask<Result<CreditCardInfo>> GetCreditCard(Data.Bookings.Booking booking, BookingAvailabilityInfo availabilityInfo)
        {
            if (availabilityInfo.CardRequirement is null)
                return null;
            
            _logger.LogVccIssueStarted(booking.ReferenceCode);

            return await _creditCardProvider.Get(referenceCode: booking.ReferenceCode,
                moneyAmount: availabilityInfo.OriginalSupplierPrice,
                activationDate: availabilityInfo.CardRequirement.Value.ActivationDate,
                dueDate: availabilityInfo.CardRequirement.Value.DueDate,
                supplier: (int) availabilityInfo.Supplier,
                accommodationName: booking.AccommodationName);
        }


        private const int BookExecutionTimeLimitInSeconds = 30;


        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IBookingResponseProcessor _responseProcessor;
        private readonly IBookingAnalyticsService _bookingAnalyticsService;
        private readonly IBookingRecordsUpdater _bookingRecordsUpdater;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRequestStorage _requestStorage;
        private readonly ICreditCardProvider _creditCardProvider;
        private readonly ILogger<BookingRequestExecutor> _logger;
    }
}