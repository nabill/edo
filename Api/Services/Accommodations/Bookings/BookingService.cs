using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;
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
            IBookingRecordsManager bookingRecordsManager,
            IBookingAuditLogService bookingAuditLogService,
            ISupplierOrderService supplierOrderService,
            EdoContext context,
            IBookingMailingService bookingMailingService,
            ILogger<BookingService> logger,
            IProviderRouter providerRouter,
            IBookingDocumentsService documentsService,
            IBookingPaymentService paymentService)
        {
            _availabilityResultsCache = availabilityResultsCache;
            _bookingRecordsManager = bookingRecordsManager;
            _bookingAuditLogService = bookingAuditLogService;
            _supplierOrderService = supplierOrderService;
            _context = context;
            _bookingMailingService = bookingMailingService;
            _logger = logger;
            _providerRouter = providerRouter;
            _documentsService = documentsService;
            _paymentService = paymentService;
        }

        
        public async Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest, string languageCode)
        {
            var (_, isCachedAvailabilityFailure, responseWithMarkup, cachedAvailabilityError) = await _availabilityResultsCache.Get(bookingRequest.DataProvider, bookingRequest.AvailabilityId);
            if (isCachedAvailabilityFailure)
                return ProblemDetailsBuilder.Fail<string>(cachedAvailabilityError);
            
            var bookingAvailability = ExtractBookingAvailabilityInfo(responseWithMarkup.Data);
            
            var referenceCode = await _bookingRecordsManager.Register(bookingRequest, bookingAvailability, languageCode);
            return Result.Ok<string, ProblemDetails>(referenceCode);
        }
        
        
        public async Task<Result<AccommodationBookingInfo, ProblemDetails>> Finalize(string referenceCode, AgentContext agent, string languageCode)
        {
            var (_, isFailure, booking, error) = await _bookingRecordsManager.GetAgentsBooking(referenceCode);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<AccommodationBookingInfo>(error);

            if (!agent.IsUsingAgency(booking.AgencyId))
                return ProblemDetailsBuilder.Fail<AccommodationBookingInfo>("The booking does not belong to your current agency");

            if (booking.PaymentStatus == BookingPaymentStatuses.NotPaid)
            {
                _logger.LogBookingFinalizationPaymentFailure($"The booking with the reference code: '{referenceCode}' hasn't been paid");
                return ProblemDetailsBuilder.Fail<AccommodationBookingInfo>("The booking hasn't been paid");
            }

            return await BookOnProvider()
                .Tap(details => ProcessResponse(details, booking))
                .Tap(GenerateInvoice)
                .OnFailure(VoidMoney)
                .Bind(GetBookingInfo);

            
            async Task<Result<BookingDetails, ProblemDetails>> BookOnProvider()
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

                    var bookingResult = await _providerRouter.Book(booking.DataProvider, innerRequest, languageCode);
                    if(bookingResult.IsFailure)
                        _logger.LogBookingFinalizationFailure($"The booking finalization with the reference code: '{referenceCode}' has been failed");

                    return bookingResult;
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Failed to update booking data (refcode '{referenceCode}') after the request to the connector";

                    var (_, isCancellationFailed, cancellationError) = await _providerRouter.CancelBooking(booking.DataProvider, booking.ReferenceCode);
                    if (isCancellationFailed)
                        errorMessage += Environment.NewLine + $"Booking cancellation has failed: {cancellationError}";

                    _logger.LogBookingFinalizationFailure(errorMessage);

                    return ProblemDetailsBuilder.Fail<BookingDetails>(
                        $"Cannot update booking data (refcode '{referenceCode}') after the request to the connector");
                }
            }
            
            
            Task<Result> GenerateInvoice(BookingDetails _) => _documentsService.GenerateInvoice(booking.Id);


            Task VoidMoney(ProblemDetails problemDetails) => _paymentService.VoidMoney(booking, agent.ToUserInfo());
            
            
            Task<Result<AccommodationBookingInfo, ProblemDetails>> GetBookingInfo(BookingDetails details)
            {
                return _bookingRecordsManager.GetAgentBookingInfo(details.ReferenceCode, languageCode)
                    .ToResultWithProblemDetails();
            }
        }
        
        
        public async Task ProcessResponse(BookingDetails bookingResponse, Booking booking)
        {
            if (bookingResponse.Status == booking.Status)
                return;
            
            await _bookingAuditLogService.Add(bookingResponse, booking);
            
            _logger.LogBookingResponseProcessStarted($"Start the booking response processing with the reference code '{bookingResponse.ReferenceCode}'");
            
            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Confirmed:
                    await ConfirmBooking();
                    break;
                case BookingStatusCodes.Cancelled:
                    await CancelBooking(booking);
                    break;
                default: 
                    await UpdateBookingDetails();
                    break;
            }

            _logger.LogBookingResponseProcessSuccess(
                $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has been successfully processed");
            
            async Task ConfirmBooking()
            {
                await _bookingRecordsManager.Confirm(bookingResponse, booking);
                await SaveSupplierOrder();
            }


            Task UpdateBookingDetails() => _bookingRecordsManager.UpdateBookingDetails(bookingResponse, booking);

            
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
        
        private async Task CancelBooking(Booking booking)
        {
            await _bookingRecordsManager.ConfirmBookingCancellation(booking);
            await NotifyAgent();
            await CancelSupplierOrder();
            
            async Task CancelSupplierOrder()
            {
                var referenceCode = booking.ReferenceCode;
                await _supplierOrderService.Cancel(referenceCode);
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
        }
        
      
        public async Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, AgentContext agent)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingId, agent.AgentId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(getBookingError);

            if (!agent.IsUsingAgency(booking.AgencyId))
                return ProblemDetailsBuilder.Fail<VoidObject>("The booking does not belong to your current agency");

            return await ProcessBookingCancellation(booking, agent.ToUserInfo());
        }
        
        
        public async Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, ServiceAccount serviceAccount)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(getBookingError);

            return await ProcessBookingCancellation(booking, serviceAccount.ToUserInfo());
        }
        
        
        public async Task<Result<BookingDetails, ProblemDetails>> RefreshStatus(int bookingId)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(getBookingError);

            var refCode = booking.ReferenceCode;
            var (_, isGetDetailsFailure, newDetails, getDetailsError) = await _providerRouter.GetBookingDetails(booking.DataProvider, refCode, booking.LanguageCode);
            if(isGetDetailsFailure)
                return Result.Failure<BookingDetails, ProblemDetails>(getDetailsError);
            
            await _bookingRecordsManager.UpdateBookingDetails(newDetails, booking);
            return Result.Ok<BookingDetails, ProblemDetails>(newDetails);
        }


        private async Task<Result<VoidObject, ProblemDetails>> ProcessBookingCancellation(Booking booking, UserInfo user)
        {
            if (booking.Status == BookingStatusCodes.Cancelled)
                return Result.Ok<VoidObject, ProblemDetails>(VoidObject.Instance);
            
            var (_, isFailure, _, error) = await SendCancellationRequest()
                .Bind(VoidMoney)
                .Tap(SetBookingCancelled);

            return isFailure
                ? ProblemDetailsBuilder.Fail<VoidObject>(error.Detail)
                : Result.Ok<VoidObject, ProblemDetails>(VoidObject.Instance);


            Task SetBookingCancelled(Booking b) => _bookingRecordsManager.ConfirmBookingCancellation(b);


            async Task<Result<Booking, ProblemDetails>> SendCancellationRequest()
            {
                var (_, isCancelFailure, _, cancelError) = await _providerRouter.CancelBooking(booking.DataProvider, booking.ReferenceCode);
                return isCancelFailure
                    ? Result.Failure<Booking, ProblemDetails>(cancelError)
                    : Result.Ok<Booking, ProblemDetails>(booking);
            }


            async Task<Result<Booking, ProblemDetails>> VoidMoney(Booking b)
            {
                var (_, isVoidMoneyFailure, voidError) = await _paymentService.VoidMoney(b, user);

                return isVoidMoneyFailure
                    ? ProblemDetailsBuilder.Fail<Booking>(voidError)
                    : Result.Ok<Booking, ProblemDetails>(b);
            }
        }


        private BookingAvailabilityInfo ExtractBookingAvailabilityInfo(SingleAccommodationAvailabilityDetailsWithDeadline response)
        {
            var location = response.AccommodationDetails.Location;
            
            return new BookingAvailabilityInfo(
                response.AccommodationDetails.Id,
                response.AccommodationDetails.Name,
                response.RoomContractSet,
                location.LocalityZoneCode,
                location.LocalityZone,
                location.LocalityCode,
                location.Locality,
                location.CountryCode,
                location.Country,
                location.Address,
                location.Coordinates,
                response.CheckInDate,
                response.CheckOutDate,
                response.NumberOfNights);
        }
        
        
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingAuditLogService _bookingAuditLogService;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly EdoContext _context;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly ILogger<BookingService> _logger;
        private readonly IProviderRouter _providerRouter;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingPaymentService _paymentService;
    }
}