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


        public async Task<Booking> Execute(AccommodationBookingRequest bookingRequest, string availabilityId, Data.Bookings.Booking booking, AgentContext agent, string languageCode)
        {
            var response = await SendSupplierRequest(bookingRequest, availabilityId, booking, languageCode);
            _analyticsService.LogBookingOccured(bookingRequest, booking, agent);
            await ProcessResponse(response);
            return response;
            
            async Task<EdoContracts.Accommodations.Booking> SendSupplierRequest(AccommodationBookingRequest bookingRequest, string availabilityId,
                Data.Bookings.Booking booking, string languageCode)
            {
                var features = new List<Feature>(); //bookingRequest.Features

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
                    var bookingResult = await _supplierConnectorManager
                        .Get(booking.Supplier)
                        .Book(innerRequest, languageCode);

                    if (bookingResult.IsSuccess)
                    {
                        return bookingResult.Value;
                    }

                    // If result is failed this does not mean that booking failed. This means that we should check it later.
                    _logger.LogBookingFinalizationFailure($"The booking finalization with the reference code: '{booking.ReferenceCode}' has been failed");
                    return GetStubDetails(booking);
                }
                catch
                {
                    var errorMessage = $"Failed to update booking data (refcode '{booking.ReferenceCode}') after the request to the connector";

                    var (_, isCancellationFailed, cancellationError) =
                        await _supplierConnectorManager.Get(booking.Supplier).CancelBooking(booking.ReferenceCode);
                    if (isCancellationFailed)
                        errorMessage += Environment.NewLine + $"Booking cancellation has failed: {cancellationError}";

                    _logger.LogBookingFinalizationFailure(errorMessage);

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
                        BookingUpdateModes.Asynchronous);
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