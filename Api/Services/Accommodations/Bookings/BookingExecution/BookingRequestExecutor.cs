using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.Errors;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public class BookingRequestExecutor : IBookingRequestExecutor
    {
        public BookingRequestExecutor(ISupplierConnectorManager supplierConnectorManager,
            IBookingResponseProcessor responseProcessor,
            AvailabilityAnalyticsService analyticsService,
            ILogger<BookingRequestExecutor> logger)
        {
            _supplierConnectorManager = supplierConnectorManager;
            _responseProcessor = responseProcessor;
            _analyticsService = analyticsService;
            _logger = logger;
        }


        public async Task<Result<Booking>> Execute(AccommodationBookingRequest bookingRequest, string availabilityId, Data.Bookings.Booking booking, AgentContext agent, string languageCode)
        {
            var bookingRequestResult = await SendSupplierRequest(bookingRequest, availabilityId, booking, languageCode);
            if (bookingRequestResult.IsFailure)
                return bookingRequestResult;
            
            _analyticsService.LogBookingOccured(bookingRequest, booking, agent);
            await ProcessResponse(bookingRequestResult.Value);
            return bookingRequestResult.Value;
            
            async Task<Result<Booking>> SendSupplierRequest(AccommodationBookingRequest bookingRequest, string availabilityId,
                Data.Bookings.Booking booking, string languageCode)
            {
                var features = new List<Feature>();

                var roomDetails = bookingRequest.RoomDetails
                    .Select(d => new SlimRoomOccupation(d.Type, d.Passengers, string.Empty, d.IsExtraBedNeeded))
                    .ToList();

                var innerRequest = new BookingRequest(availabilityId,
                    bookingRequest.RoomContractSetId,
                    booking.ReferenceCode,
                    roomDetails,
                    features,
                    bookingRequest.RejectIfUnavailable);

                try
                {
                    var (isSuccess, _, bookingResult, error) = await _supplierConnectorManager
                        .Get(booking.Supplier)
                        .Book(innerRequest, languageCode);

                    if (isSuccess)
                        return bookingResult;

                    var message = error.Detail;
                    // If result is failed this does not mean that booking failed. All known cases are listed below
                    _logger.LogBookingFinalizationFailure($"The booking finalization with the reference code: '{booking.ReferenceCode}' has been failed with a message: {message}");

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
                }
                catch (Exception ex)
                {
                    _logger.LogBookingFinalizationException(ex);
                    return GetStubDetails(booking);
                }


                // TODO: Remove room information and contract description from booking NIJO-915
                static EdoContracts.Accommodations.Booking GetStubDetails(Data.Bookings.Booking booking)
                    => new EdoContracts.Accommodations.Booking(booking.ReferenceCode,
                        // Will be set in the refresh step
                        BookingStatusCodes.WaitingForResponse,
                        booking.AccommodationId,
                        booking.SupplierReferenceCode,
                        booking.CheckInDate,
                        booking.CheckOutDate,
                        new List<SlimRoomOccupation>(0),
                        booking.UpdateMode);
            }
            
            
            Task ProcessResponse(Booking response) 
                => _responseProcessor.ProcessResponse(response);
        }


        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IBookingResponseProcessor _responseProcessor;
        private readonly AvailabilityAnalyticsService _analyticsService;
        private readonly ILogger<BookingRequestExecutor> _logger;
    }
}