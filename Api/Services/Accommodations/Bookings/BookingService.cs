using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingService : IBookingService
    {
        public BookingService(IAvailabilityResultsCache availabilityResultsCache,
            IBookingManager bookingManager,
            IBookingAuditLogService bookingAuditLogService,
            ISupplierOrderService supplierOrderService,
            EdoContext context,
            IBookingMailingService bookingMailingService,
            ILogger<BookingService> logger,
            IProviderRouter providerRouter,
            IServiceAccountContext serviceAccountContext,
            IAgentContext agentContext,
            IBookingPaymentService paymentService)
        {
            _availabilityResultsCache = availabilityResultsCache;
            _bookingManager = bookingManager;
            _bookingAuditLogService = bookingAuditLogService;
            _supplierOrderService = supplierOrderService;
            _context = context;
            _bookingMailingService = bookingMailingService;
            _logger = logger;
            _providerRouter = providerRouter;
            _serviceAccountContext = serviceAccountContext;
            _agentContext = agentContext;
            _paymentService = paymentService;
        }

        
        public async Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest, string languageCode)
        {
            var (_, isCachedAvailabilityFailure, responseWithMarkup, cachedAvailabilityError) = await _availabilityResultsCache.Get(bookingRequest.DataProvider, bookingRequest.AvailabilityId);
            if (isCachedAvailabilityFailure)
                return ProblemDetailsBuilder.Fail<string>(cachedAvailabilityError);
            
            var bookingAvailability = ExtractBookingAvailabilityInfo(responseWithMarkup.Data);
            
            var (_, isFailure, referenceCode, error) = await _bookingManager.Register(bookingRequest, bookingAvailability, languageCode);
            
            return isFailure 
                ? ProblemDetailsBuilder.Fail<string>(error) 
                : Result.Ok<string, ProblemDetails>(referenceCode);
        }
        
        
        public async Task<Result<BookingDetails, ProblemDetails>> Finalize(string referenceCode, string languageCode)
        {
            // TODO: Refactor and simplify method
            var (_, isFailure, booking, error) = await _bookingManager.GetAgentsBooking(referenceCode);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(error);

            if (booking.PaymentStatus == BookingPaymentStatuses.NotPaid)
            {
                _logger.LogBookingFinalizationFailedToPay($"The booking with the reference code: '{referenceCode}' hasn't been paid");
                return ProblemDetailsBuilder.Fail<BookingDetails>("The booking hasn't been paid");
            }

            var (_, isBookingFailure, bookingDetails, bookingError) = await SendBookingRequest()
                    .OnSuccess(details => _bookingManager.Finalize(booking, details))
                .OnFailure(VoidMoney);

            if (isBookingFailure)
            {
                _logger.LogBookingFinalizationFailed($"The booking finalization with the reference code: '{referenceCode}' has been failed");
                return ProblemDetailsBuilder.Fail<BookingDetails>(bookingError.Detail);
            }

            var processingResult = await ProcessResponse(bookingDetails);
            
            return processingResult.IsFailure 
                ? ProblemDetailsBuilder.Fail<BookingDetails>(processingResult.Error) 
                : Result.Ok<BookingDetails, ProblemDetails>(bookingDetails);

         
            async Task<Result<BookingDetails, ProblemDetails>> SendBookingRequest()
            {
                try
                {
                    // TODO: will be implemented in NIJO-31 
                    var bookingRequest = JsonConvert.DeserializeObject<AccommodationBookingRequest>(booking.BookingRequest);

                    var features = new List<Feature>(); //bookingRequest.Features

                    var roomDetails = bookingRequest.RoomDetails
                        .Select(d => new SlimRoomDetails(d.Type, d.Passengers, d.IsExtraBedNeeded))
                        .ToList();

                    var innerRequest = new BookingRequest(bookingRequest.AvailabilityId,
                        bookingRequest.RoomContractSetId,
                        bookingRequest.Nationality,
                        bookingRequest.PaymentMethod,
                        booking.ReferenceCode,
                        bookingRequest.Residency,
                        roomDetails,
                        features,
                        bookingRequest.RejectIfUnavailable);

                    return await _providerRouter.Book(booking.DataProvider, innerRequest, languageCode);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Failed to update booking data (refcode '{referenceCode}') after the request to the connector";

                    var (_, isCancellationFailed, cancellationError) = await _providerRouter.CancelBooking(booking.DataProvider, booking.ReferenceCode);
                    if (isCancellationFailed)
                        errorMessage += Environment.NewLine + $"Booking cancellation has failed: {cancellationError}";

                    _logger.LogBookingFinalizationFailed(errorMessage);

                    return ProblemDetailsBuilder.Fail<BookingDetails>(
                        $"Cannot update booking data (refcode '{referenceCode}') after the request to the connector");
                }
            }


            Task VoidMoney(ProblemDetails problemDetails) => _paymentService.VoidMoney(booking);
        }
        
        
        public async Task<Result> ProcessResponse(BookingDetails bookingResponse, Data.Booking.Booking booking = null)
        {
            if (booking is null)
            {
                var (_, isFailure, bookingData, error) = await _bookingManager.Get(bookingResponse.ReferenceCode);
                if (isFailure)
                {
                    _logger.LogBookingProcessResponseFailed($"The booking response with the reference code '{bookingResponse.ReferenceCode}' isn't related with any db record");
                    return Result.Fail(error);
                }

                booking = bookingData;
            }

            if (bookingResponse.Status == booking.Status)
                return Result.Ok();
            
            await _bookingAuditLogService.Add(bookingResponse, booking);
            
            _logger.LogBookingProcessResponseStarted($"Start the booking response processing with the reference code '{bookingResponse.ReferenceCode}'");
            
            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Rejected:
                    await UpdateBookingDetails();
                    break;
                case BookingStatusCodes.Pending:
                case BookingStatusCodes.Confirmed:
                    await ConfirmBooking();
                    break;
                case BookingStatusCodes.Cancelled:
                    await CancelBooking();
                    break;
            }

            return Result.Ok();

            
            async Task ConfirmBooking()
            {
                await _bookingManager.ConfirmBooking(bookingResponse, booking);
                await SaveSupplierOrder();
            }

            
            async Task CancelBooking()
            {
                await _bookingManager.ConfirmBookingCancellation(bookingResponse, booking);
                await NotifyAgent();
                await CancelSupplierOrder();
            }


            Task UpdateBookingDetails() => _bookingManager.UpdateBookingDetails(bookingResponse, booking);


            Task CancelSupplierOrder()
            {
                var referenceCode = booking.ReferenceCode;
                return _supplierOrderService.Cancel(referenceCode);
            }
            

            async Task NotifyAgent()
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                if (agent == default)
                {
                    _logger.LogWarning("Booking cancellation notification: could not find agent with id '{0}' for the booking '{1}'",
                        booking.AgentId, booking.ReferenceCode);
                    return;
                }

                await _bookingMailingService.NotifyBookingCancelled(booking.ReferenceCode, agent.Email, $"{agent.LastName} {agent.FirstName}");
            }

          
            async Task SaveSupplierOrder()
            {
                var supplierPrice = bookingResponse.RoomContractSet.Price.NetTotal;
                await _supplierOrderService.Add(bookingResponse.ReferenceCode, ServiceTypes.HTL, supplierPrice);
            }
            
            //TICKET https://happytravel.atlassian.net/browse/NIJO-315
            /*
            async Task<Result> LogAppliedMarkups()
            {
                long availabilityId = ??? ;
                
                var (_, isGetAvailabilityFailure, responseWithMarkup, cachedAvailabilityError) = await _availabilityResultsCache.Get(availabilityId);
                if (isGetAvailabilityFailure)
                    return Result.Fail(cachedAvailabilityError);

                await _markupLogger.Write(bookingResponse.ReferenceCode, ServiceTypes.HTL, responseWithMarkup.AppliedPolicies);
                return Result.Ok();
            }
            */
        }
        
      
        public async Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingManager.Get(bookingId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(getBookingError);

            var (isServiceAccount, _, _, _) = await _serviceAccountContext.GetUserInfo();
            if (!isServiceAccount)
            {
                var (_, isFailure, _, error) = await _agentContext.GetUserInfo();
                if (isFailure)
                    return ProblemDetailsBuilder.Fail<VoidObject>(error);
            }
            

            if (booking.Status == BookingStatusCodes.Cancelled)
                return ProblemDetailsBuilder.Fail<VoidObject>("Booking was already cancelled");

            var (_, isCancellationFailure, _, cancellationError) = await ExecuteBookingCancellation();

            return isCancellationFailure
                ? ProblemDetailsBuilder.Fail<VoidObject>(cancellationError.Detail)
                : Result.Ok<VoidObject, ProblemDetails>(VoidObject.Instance);

            Task<Result<VoidObject, ProblemDetails>> ExecuteBookingCancellation() => _providerRouter.CancelBooking(booking.DataProvider, booking.ReferenceCode);
        }


        public async Task<Result<BookingDetails, ProblemDetails>> Refresh(int bookingId)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingManager.Get(bookingId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(getBookingError);

            var refCode = booking.ReferenceCode;
            var (_, isGetDetailsFailure, newDetails, getDetailsError) = await _providerRouter.GetBookingDetails(booking.DataProvider, refCode, booking.LanguageCode);
            if(isGetDetailsFailure)
                return Result.Fail<BookingDetails, ProblemDetails>(getDetailsError);
            
            await _bookingManager.UpdateBookingDetails(newDetails, booking);
            return Result.Ok<BookingDetails, ProblemDetails>(newDetails);
        }


        private async Task<Result<VoidObject, ProblemDetails>> SendCancellationBookingRequest(Data.Booking.Booking booking)
        {
            var (_, isFailure, _, error) = await SendCancellationRequest()
                .OnSuccessWithTransaction(_context, b =>
                    VoidMoney(b)
                        .OnSuccess(() => ProcessResponse(b))
                );

            return isFailure
                ? ProblemDetailsBuilder.Fail<VoidObject>(error.Detail)
                : Result.Ok<VoidObject, ProblemDetails>(VoidObject.Instance);


            async Task<Result<Data.Booking.Booking, ProblemDetails>> ProcessResponse(Data.Booking.Booking b)
            {
                var bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(b.BookingDetails);

                var responseResult = await this.ProcessResponse(
                    new BookingDetails(bookingDetails.ReferenceCode,
                        bookingDetails.AgentReference,
                        BookingStatusCodes.Cancelled,
                        bookingDetails.AccommodationId,
                        bookingDetails.BookingCode,
                        bookingDetails.CheckInDate,
                        bookingDetails.CheckOutDate,
                        bookingDetails.ContractDescription,
                        bookingDetails.Deadline,
                        bookingDetails.Locality,
                        bookingDetails.TariffCode,
                        bookingDetails.RoomDetails,
                        bookingDetails.LocationDescription,
                        bookingDetails.RoomContractSet), 
                    b);

                return responseResult.IsFailure 
                    ? ProblemDetailsBuilder.Fail<Data.Booking.Booking>(responseResult.Error)
                    : Result.Ok<Data.Booking.Booking, ProblemDetails>(b);
            }


            async Task<Result<Data.Booking.Booking, ProblemDetails>> SendCancellationRequest()
            {
                var (_, isCancelFailure, _, cancelError) = await _providerRouter.CancelBooking(booking.DataProvider, booking.ReferenceCode);
                return isCancelFailure
                    ? Result.Fail<Data.Booking.Booking, ProblemDetails>(cancelError)
                    : Result.Ok<Data.Booking.Booking, ProblemDetails>(booking);
            }


            async Task<Result<Data.Booking.Booking, ProblemDetails>> VoidMoney(Data.Booking.Booking b)
            {
                var (_, isVoidMoneyFailure, voidError) = await _paymentService.VoidMoney(b);

                return isVoidMoneyFailure
                    ? ProblemDetailsBuilder.Fail<Data.Booking.Booking>(voidError)
                    : Result.Ok<Data.Booking.Booking, ProblemDetails>(b);
            }
        }


        private BookingAvailabilityInfo ExtractBookingAvailabilityInfo(SingleAccommodationAvailabilityDetailsWithDeadline response)
        {
            return new BookingAvailabilityInfo(
                response.AccommodationDetails.Id,
                response.AccommodationDetails.Name,
                response.RoomContractSet,
                response.AccommodationDetails.Location.LocalityCode,
                response.AccommodationDetails.Location.Locality,
                response.AccommodationDetails.Location.CountryCode,
                response.AccommodationDetails.Location.Country,
                response.CheckInDate,
                response.CheckOutDate);
        }
        
        
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IBookingManager _bookingManager;
        private readonly IBookingAuditLogService _bookingAuditLogService;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly EdoContext _context;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly ILogger<BookingService> _logger;
        private readonly IProviderRouter _providerRouter;
        private readonly IServiceAccountContext _serviceAccountContext;
        private readonly IAgentContext _agentContext;
        private readonly IBookingPaymentService _paymentService;
    }
}