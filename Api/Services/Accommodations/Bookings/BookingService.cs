using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Markups.Availability;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingService : IBookingService
    {
        public BookingService(ICustomerContext customerContext,
            IPermissionChecker permissionChecker,
            IAvailabilityResultsCache availabilityResultsCache,
            IBookingManager bookingManager,
            IBookingAuditLogService bookingAuditLogService,
            ISupplierOrderService supplierOrderService,
            EdoContext context,
            IBookingMailingService bookingMailingService,
            ILogger<BookingService> logger,
            IPaymentService paymentService,
            IDeadlineDetailsCache deadlineDetailsCache)
        {
            _customerContext = customerContext;
            _permissionChecker = permissionChecker;
            _availabilityResultsCache = availabilityResultsCache;
            _bookingManager = bookingManager;
            _bookingAuditLogService = bookingAuditLogService;
            _supplierOrderService = supplierOrderService;
            _context = context;
            _bookingMailingService = bookingMailingService;
            _logger = logger;
            _paymentService = paymentService;
            _deadlineDetailsCache = deadlineDetailsCache;
        }

        
        public async Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest)
        {
            var (_, isCustomerFailure, customerInfo, customerError) = await _customerContext.GetCustomerInfo();
            if (isCustomerFailure)
            {
                _logger.LogWarning("Failed to get the customer: {0}", customerError);
                return ProblemDetailsBuilder.Fail<string>(customerError);
            }
            
            var (_, permissionDenied, permissionError) = await _permissionChecker
                .CheckInCompanyPermission(customerInfo, InCompanyPermissions.AccommodationBooking);
            if (permissionDenied)
            {
                _logger.LogWarning( "The customer with {0}: '{1}' has failed to get permissions: {2}",  nameof(customerInfo.CustomerId), customerInfo.CustomerId, permissionError);
                return ProblemDetailsBuilder.Fail<string>(permissionError);
            }
            
            var (_, isCachedAvailabilityFailure, responseWithMarkup, cachedAvailabilityError) = await _availabilityResultsCache.Get(bookingRequest.DataProvider, bookingRequest.AvailabilityId);
            if (isCachedAvailabilityFailure)
                return ProblemDetailsBuilder.Fail<string>(cachedAvailabilityError);
            
            var (_, isGetAvailabilityFailure, bookingAvailability, getBookingAvailabilityError) = GetBookingAvailability(responseWithMarkup, bookingRequest.AgreementId);
            if (isGetAvailabilityFailure)
                return  ProblemDetailsBuilder.Fail<string>(getBookingAvailabilityError.Detail);
            
            var (_, isFailure, referenceCode, error) = await _bookingManager.Register(bookingRequest, bookingAvailability);
            
            return isFailure 
                ? ProblemDetailsBuilder.Fail<string>(error) 
                : Result.Ok<string, ProblemDetails>(referenceCode);
        }
        
        
        //TODO Add logging methods to LoggerExtensions class 
        public async Task<Result<BookingDetails, ProblemDetails>> Finalize(string referenceCode, string languageCode)
        {
            // TODO: Refactor and simplify method
            var (_, isFailure, booking, error) = await _bookingManager.GetCustomersBooking(referenceCode);
            if (isFailure)
                ProblemDetailsBuilder.Fail<BookingDetails>(error);

            if (booking.PaymentStatus == BookingPaymentStatuses.NotPaid)
            {
                _logger.LogWarning("The booking with the {0}: '{1}' hasn't been paid",
                    nameof(Data.Booking.Booking.ReferenceCode), nameof(Data.Booking.Booking.ReferenceCode));
                return ProblemDetailsBuilder.Fail<BookingDetails>("The booking hasn't been paid");
            }

            var (_, isBookingFailure, bookingDetails, bookingError) = await FinalizeBooking()
                .OnFailure(VoidMoney);

            if (isBookingFailure)
            {
                _logger.LogInformation("The booking finalization with the {0}: '{1}' has been failed",nameof(Data.Booking.Booking.ReferenceCode), nameof(Data.Booking.Booking.ReferenceCode));
                return ProblemDetailsBuilder.Fail<BookingDetails>(bookingError.Detail);
            }

            var processingResult = await ProcessResponse(bookingDetails);
            
            return processingResult.IsFailure 
                ? ProblemDetailsBuilder.Fail<BookingDetails>(processingResult.Error) 
                : Result.Ok<BookingDetails, ProblemDetails>(bookingDetails);

         
            async Task<Result<BookingDetails, ProblemDetails>> FinalizeBooking()
            {
                return await _bookingManager.Finalize(booking, languageCode);
            }


            async Task VoidMoney(ProblemDetails problemDetails)
            {
                var (_, isVoidFailure, voidError) = await _paymentService.VoidMoney(booking);
                if (isVoidFailure)
                    _logger.LogError(problemDetails.Detail + Environment.NewLine + voidError);
            }
        }
        
        
        //TODO Add logging methods to LoggerExtensions class 
        public async Task<Result> ProcessResponse(BookingDetails bookingResponse, Data.Booking.Booking booking = null)
        {
            if (booking is null)
            {
                var (_, isFailure, bookingData, error) = await _bookingManager.Get(bookingResponse.ReferenceCode);
                if (isFailure)
                {
                    _logger.LogWarning("The booking response with the {0} '{1}' isn't related with any db record",
                        nameof(bookingResponse.ReferenceCode), bookingResponse.ReferenceCode);
                    return Result.Fail(error);
                }

                booking = bookingData;
            }

            if (bookingResponse.Status == booking.Status)
                return Result.Ok();
            
            await _bookingAuditLogService.Add(bookingResponse, booking);
            
            _logger.LogInformation("Start the booking response processing with the {0} '{1}'", nameof(bookingResponse.ReferenceCode), bookingResponse.ReferenceCode);
            
            Result result = default; 
            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Rejected:
                    result = await UpdateBookingDetails();
                    break;
                case BookingStatusCodes.Pending:
                case BookingStatusCodes.Confirmed:
                    result = await ConfirmBooking();
                    break;
                case BookingStatusCodes.Cancelled:
                    result = await CancelBooking();
                    break;
            }

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "The booking response with the {0} '{1}' has been successfully processed",nameof(bookingResponse.ReferenceCode), bookingResponse.ReferenceCode);
                return result;
            }

            _logger.LogWarning("The booking response with the {0} '{1}' hasn't been processed because of {2}",
                nameof(bookingResponse.ReferenceCode), bookingResponse.ReferenceCode, result.Error);

            return Result.Fail("The booking response hasn't been processed");
            
            Task<Result> ConfirmBooking()
            {
                return _bookingManager
                    .ConfirmBooking(bookingResponse, booking)
                    .OnSuccess(SaveSupplierOrder);
                //.OnSuccess(LogAppliedMarkups);
            }
            
            
            Task<Result> CancelBooking()
            {
                return _bookingManager.ConfirmBookingCancellation(bookingResponse, booking)
                    .OnSuccess(NotifyCustomer)
                    .OnSuccess(CancelSupplierOrder);
            }


            Task<Result> UpdateBookingDetails()
            {
                return _bookingManager.UpdateBookingDetails(bookingResponse, booking);
            }
            
            
            async Task CancelSupplierOrder()
            {
                var referenceCode = booking.ReferenceCode;
                await _supplierOrderService.Cancel(referenceCode);
            }
            

            async Task NotifyCustomer()
            {
                var customer = await _context.Customers.SingleOrDefaultAsync(c => c.Id == booking.CustomerId);
                if (customer == default)
                {
                    _logger.LogWarning("Booking cancellation notification: could not find customer with id '{0}' for the booking '{1}'",
                        booking.CustomerId, booking.ReferenceCode);
                    return;
                }

                await _bookingMailingService.NotifyBookingCancelled(booking.ReferenceCode, customer.Email, $"{customer.LastName} {customer.FirstName}");
            }

          
            async Task SaveSupplierOrder()
            {
                var supplierPrice = bookingResponse.Agreement.Price.NetTotal;
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
        
        
        public Task<Result<AccommodationBookingInfo>> Get(int bookingId) => _bookingManager.GetCustomerBookingInfo(bookingId);


        public Task<Result<AccommodationBookingInfo>> Get(string referenceCode) => _bookingManager.GetCustomerBookingInfo(referenceCode);


        public Task<Result<List<SlimAccommodationBookingInfo>>> Get() => _bookingManager.GetCustomerBookingsInfo();


        public async Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId)
        {
            var (_, isFailure, booking, error) = await _bookingManager.Get(bookingId);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(error);
                    
            return await SendCancellationBookingRequest(booking);
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
                        bookingDetails.Agreement), 
                    b);

                return responseResult.IsFailure 
                    ? ProblemDetailsBuilder.Fail<Data.Booking.Booking>(responseResult.Error)
                    : Result.Ok<Data.Booking.Booking, ProblemDetails>(b);
            }


            Task<Result<Data.Booking.Booking, ProblemDetails>> SendCancellationRequest() => _bookingManager.CancelBooking(booking.Id);


            async Task<Result<Data.Booking.Booking, ProblemDetails>> VoidMoney(Data.Booking.Booking b)
            {
                var (_, isVoidMoneyFailure, voidError) = await _paymentService.VoidMoney(b);

                return isVoidMoneyFailure
                    ? ProblemDetailsBuilder.Fail<Data.Booking.Booking>(voidError)
                    : Result.Ok<Data.Booking.Booking, ProblemDetails>(b);
            }
        }


        private Result<BookingAvailabilityInfo, ProblemDetails> GetBookingAvailability(
            SingleAccommodationAvailabilityDetailsWithMarkup responseWithMarkup, Guid agreementId)
        {
            var availability = ExtractBookingAvailabilityInfo(responseWithMarkup.ResultResponse, agreementId);
            if (availability.Equals(default))
                return ProblemDetailsBuilder.Fail<BookingAvailabilityInfo>("Could not find the availability by given id");
            
            var (_, getDeadlineDetailsFailure, deadlineDetails, deadlineDetailsError) = _deadlineDetailsCache.Get(agreementId.ToString());
            
            return getDeadlineDetailsFailure 
                ? ProblemDetailsBuilder.Fail<BookingAvailabilityInfo>($"Could not get deadline policies: {deadlineDetailsError}")
                : Result.Ok<BookingAvailabilityInfo, ProblemDetails>(availability.AddDeadlineDetails(deadlineDetails));
        }


        private BookingAvailabilityInfo ExtractBookingAvailabilityInfo(SingleAccommodationAvailabilityDetails response, Guid agreementId)
        {
            if (response.Equals(default))
                return default;

            return (from agreement in response.Agreements
                    where agreement.Id == agreementId
                    select new BookingAvailabilityInfo(
                        response.AccommodationDetails.Id,
                        response.AccommodationDetails.Name,
                        agreement,
                        response.AccommodationDetails.Location.LocalityCode,
                        response.AccommodationDetails.Location.Locality,
                        response.AccommodationDetails.Location.CountryCode,
                        response.AccommodationDetails.Location.Country,
                        response.CheckInDate,
                        response.CheckOutDate))
                .SingleOrDefault();
        }
        
        
        private readonly ICustomerContext _customerContext;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IBookingManager _bookingManager;
        private readonly IBookingAuditLogService _bookingAuditLogService;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly EdoContext _context;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly ILogger<BookingService> _logger;
        private readonly IDeadlineDetailsCache _deadlineDetailsCache;
        
        private readonly IPaymentService _paymentService;
    }
}