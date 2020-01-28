using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Markups.Availability;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Deadline;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationService : IAccommodationService
    {
        public AccommodationService(IMemoryFlow flow,
            IOptions<DataProviderOptions> options,
            IDataProviderClient dataProviderClient,
            IAccommodationBookingManager accommodationBookingManager,
            IAvailabilityResultsCache availabilityResultsCache,
            ICustomerContext customerContext,
            ICancellationPoliciesService cancellationPoliciesService,
            ISupplierOrderService supplierOrderService,
            IPermissionChecker permissionChecker,
            EdoContext context,
            IPaymentService paymentService,
            ILogger<AccommodationService> logger,
            IServiceAccountContext serviceAccountContext,
            IDateTimeProvider dateTimeProvider,
            IBookingMailingService bookingMailingService,
            IBookingAuditLogService bookingAuditLogService)
        {
            _flow = flow;
            _dataProviderClient = dataProviderClient;
            _accommodationBookingManager = accommodationBookingManager;
            _availabilityResultsCache = availabilityResultsCache;
            _customerContext = customerContext;
            _options = options.Value;
            _cancellationPoliciesService = cancellationPoliciesService;
            _supplierOrderService = supplierOrderService;
            _permissionChecker = permissionChecker;
            _context = context;
            _paymentService = paymentService;
            _logger = logger;
            _serviceAccountContext = serviceAccountContext;
            _dateTimeProvider = dateTimeProvider;
            _bookingMailingService = bookingMailingService;
            _bookingAuditLogService = bookingAuditLogService;
        }


        public ValueTask<Result<AccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(AccommodationService), "Accommodations", languageCode, accommodationId),
                async () => await _dataProviderClient.Get<AccommodationDetails>(
                    new Uri($"{_options.Netstorming}accommodations/{accommodationId}", UriKind.Absolute), languageCode),
                TimeSpan.FromDays(1));


        public async Task<Result<BookingDetails, ProblemDetails>> SendBookingRequest(AccommodationBookingRequest bookingRequest, string languageCode)
        {
            // TODO: Refactor and simplify method
            var (_, isCustomerFailure, customerInfo, customerError) = await _customerContext.GetCustomerInfo();
            if (isCustomerFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(customerError);

            var (_, permissionDenied, permissionError) = await _permissionChecker
                .CheckInCompanyPermission(customerInfo, InCompanyPermissions.AccommodationBooking);
            if (permissionDenied)
                return ProblemDetailsBuilder.Fail<BookingDetails>(permissionError);

            var (_, isCachedAvailabilityFailure, responseWithMarkup, cachedAvailabilityError) = await _availabilityResultsCache.Get(DataProviders.Netstorming, bookingRequest.AvailabilityId);
            if (isCachedAvailabilityFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(cachedAvailabilityError);

            var (_, isFailure, bookingDetails, bookingError) = await GetAvailability()
                .OnSuccess(Book);
            
            if (isFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(bookingError.Detail);

            var processingResult = await ProcessBookingResponse(bookingDetails);
            
            if (processingResult.IsFailure)
                return  ProblemDetailsBuilder.Fail<BookingDetails>(processingResult.Error);
            

            return Result.Ok<BookingDetails, ProblemDetails>(bookingDetails);
            
            
            async Task<Result<BookingAvailabilityInfo, ProblemDetails>> GetAvailability()
            {
                return await GetBookingAvailability(responseWithMarkup, bookingRequest.AvailabilityId, bookingRequest.AgreementId, languageCode);
            }


            async Task<Result<BookingDetails, ProblemDetails>> Book(BookingAvailabilityInfo bookingAvailability)
            {   
                return await _accommodationBookingManager.Book(bookingRequest, bookingAvailability, languageCode);
            }
        }


        public async Task<Result> ProcessBookingResponse(BookingDetails bookingResponse, Booking booking = null)
        {
            if (booking is null)
            {
                var (_, isFailure, bookingData, error) = await _accommodationBookingManager.Get(bookingResponse.ReferenceCode);
                if (isFailure)
                    return Result.Fail(error);

                booking = bookingData;
            }

            if (bookingResponse.Status == booking.Status)
                return Result.Ok();
            
            await _bookingAuditLogService.Add(bookingResponse, booking);
            
            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Rejected:
                    return await UpdateBookingDetails();
                case BookingStatusCodes.Pending:
                case BookingStatusCodes.Confirmed:
                    return await ConfirmBooking();
                case BookingStatusCodes.Cancelled:
                    return await CancelBooking();
            }

            return Result.Fail("Cannot process a booking response");

            
            Task<Result> ConfirmBooking()
            {
                return _accommodationBookingManager
                    .ConfirmBooking(bookingResponse, booking)
                    .OnSuccess(SaveSupplierOrder);
                    //.OnSuccess(LogAppliedMarkups);
            }
            
            
            Task<Result> CancelBooking()
            {
                return _accommodationBookingManager.ConfirmBookingCancellation(bookingResponse, booking)
                    .OnSuccess(NotifyCustomer)
                    .OnSuccess(CancelSupplierOrder);
            }


            Task<Result> UpdateBookingDetails()
            {
                return _accommodationBookingManager.UpdateBookingDetails(bookingResponse, booking);
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
        
        
        public Task<Result<AccommodationBookingInfo>> GetBooking(int bookingId) => _accommodationBookingManager.GetCustomerBookingInfo(bookingId);


        public Task<Result<AccommodationBookingInfo>> GetBooking(string referenceCode) => _accommodationBookingManager.GetCustomerBookingInfo(referenceCode);


        public Task<Result<List<SlimAccommodationBookingInfo>>> GetCustomerBookings() => _accommodationBookingManager.GetCustomerBookingsInfo();


        public async Task<Result<VoidObject, ProblemDetails>> SendCancellationBookingRequest(int bookingId)
        {
            var (_, isFailure, booking, error) = await _accommodationBookingManager.Get(bookingId);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(error);
                    
            return await SendCancellationBookingRequest(booking);
        }


        private async Task<Result<VoidObject, ProblemDetails>> SendCancellationBookingRequest(Booking booking)
        {
            var (_, isFailure, _, error) = await SendCancellationRequest()
                .OnSuccessWithTransaction(_context, b =>
                    VoidMoney(b)
                        .OnSuccess(() => ProcessResponse(b))
                );

            return isFailure
                ? ProblemDetailsBuilder.Fail<VoidObject>(error.Detail)
                : Result.Ok<VoidObject, ProblemDetails>(VoidObject.Instance);


            async Task<Result<Booking, ProblemDetails>> ProcessResponse(Booking b)
            {
                var bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(b.BookingDetails);

                var responseResult = await ProcessBookingResponse(
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
                    ? ProblemDetailsBuilder.Fail<Booking>(responseResult.Error)
                    : Result.Ok<Booking, ProblemDetails>(b);
            }


            Task<Result<Booking, ProblemDetails>> SendCancellationRequest() => _accommodationBookingManager.CancelBooking(booking.Id);


            async Task<Result<Booking, ProblemDetails>> VoidMoney(Booking b)
            {
                var (_, isVoidMoneyFailure, voidError) = await _paymentService.VoidMoney(b);

                return isVoidMoneyFailure
                    ? ProblemDetailsBuilder.Fail<Booking>(voidError)
                    : Result.Ok<Booking, ProblemDetails>(b);
            }
        }


        public async Task<Result<List<int>>> GetBookingsForCancellation(DateTime deadlineDate)
        {
            if (deadlineDate == default)
                return Result.Fail<List<int>>("Deadline date should be specified");

            var (_, isFailure, _, error) = await _serviceAccountContext.GetUserInfo();
            if (isFailure)
                return Result.Fail<List<int>>(error);

            // It’s prohibited to cancel booking after check-in date
            var currentDateUtc = _dateTimeProvider.UtcNow();
            var bookings = await _context.Bookings
                .Where(booking =>
                    BookingStatusesForCancellation.Contains(booking.Status) &&
                    PaymentStatusesForCancellation.Contains(booking.PaymentStatus) &&
                    booking.BookingDate > currentDateUtc)
                .ToListAsync();

            var dayBeforeDeadline = deadlineDate.Date.AddDays(1);
            var bookingIds = bookings
                .Where(booking =>
                {
                    var availabilityInfo = JsonConvert.DeserializeObject<BookingAvailabilityInfo>(booking.ServiceDetails);
                    return availabilityInfo.Agreement.DeadlineDate.Date <= dayBeforeDeadline;
                })
                .Select(booking => booking.Id)
                .ToList();

            return Result.Ok(bookingIds);
        }


        public async Task<Result<ProcessResult>> CancelBookings(List<int> bookingIds)
        {
            var (_, isUserFailure, _, userError) = await _serviceAccountContext.GetUserInfo();
            if (isUserFailure)
                return Result.Fail<ProcessResult>(userError);

            var bookings = await GetBookings();

            return await Validate() 
                .OnSuccess(ProcessBookings);


            Task<List<Booking>> GetBookings()
            {
                var ids = bookingIds;
                return _context.Bookings.Where(booking => ids.Contains(booking.Id)).ToListAsync();
            }


            Result Validate()
            {
                return bookings.Count != bookingIds.Count
                    ? Result.Fail("Invalid booking ids. Could not find some of requested bookings.")
                    : Result.Combine(bookings.Select(CheckCanBeCancelled).ToArray());


                Result CheckCanBeCancelled(Booking booking)
                    => GenericValidator<Booking>.Validate(v =>
                    {
                        v.RuleFor(c => c.PaymentStatus)
                            .Must(status => PaymentStatusesForCancellation.Contains(booking.PaymentStatus))
                            .WithMessage(
                                $"Invalid payment status for the booking '{booking.ReferenceCode}': {booking.PaymentStatus}");
                        v.RuleFor(c => c.Status)
                            .Must(status => BookingStatusesForCancellation.Contains(status))
                            .WithMessage($"Invalid booking status for the booking '{booking.ReferenceCode}': {booking.Status}");
                    }, booking);
            }


            Task<ProcessResult> ProcessBookings()
            {
                return Combine(bookings.Select(ProcessBooking));


                Task<Result<string>> ProcessBooking(Booking booking)
                {
                    return SendCancellationBookingRequest(booking.Id)
                        .OnBoth(CreateResult);


                    Result<string> CreateResult(Result<VoidObject, ProblemDetails> result)
                        => result.IsSuccess
                            ? Result.Ok($"Booking '{booking.ReferenceCode}' was cancelled.")
                            : Result.Fail<string>($"Unable to cancel booking '{booking.ReferenceCode}'. Reason: {result.Error.Detail}");
                }


                async Task<ProcessResult> Combine(IEnumerable<Task<Result<string>>> results)
                {
                    var builder = new StringBuilder();

                    foreach (var result in results)
                    {
                        var (_, isFailure, value, error) = await result;
                        builder.AppendLine(isFailure ? error : value);
                    }

                    return new ProcessResult(builder.ToString());
                }
            }
        }


        private async Task<Result<BookingAvailabilityInfo, ProblemDetails>> GetBookingAvailability(
            SingleAccommodationAvailabilityDetailsWithMarkup responseWithMarkup,
            int availabilityId, Guid agreementId, string languageCode)
        {
            var availability = ExtractBookingAvailabilityInfo(responseWithMarkup.ResultResponse, agreementId);
            if (availability.Equals(default))
                return ProblemDetailsBuilder.Fail<BookingAvailabilityInfo>("Could not find the agreement for given availability and agreement id");

            var deadlineDetailsResponse = await _cancellationPoliciesService.GetDeadlineDetails(
                availabilityId.ToString(),
                availability.AccommodationId,
                availability.Agreement.TariffCode,
                DataProviders.Netstorming,
                languageCode);

            if (deadlineDetailsResponse.IsFailure)
                return ProblemDetailsBuilder.Fail<BookingAvailabilityInfo>($"Could not get deadline policies: {deadlineDetailsResponse.Error.Detail}");

            return Result.Ok<BookingAvailabilityInfo, ProblemDetails>(availability.AddDeadlineDetails(deadlineDetailsResponse.Value));
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


        private static readonly HashSet<BookingStatusCodes> BookingStatusesForCancellation = new HashSet<BookingStatusCodes>
        {
            BookingStatusCodes.Pending, BookingStatusCodes.Confirmed
        };


        private static readonly HashSet<BookingPaymentStatuses> PaymentStatusesForCancellation = new HashSet<BookingPaymentStatuses>
        {
            BookingPaymentStatuses.NotPaid, BookingPaymentStatuses.Authorized, BookingPaymentStatuses.PartiallyAuthorized
        };


        private readonly IAccommodationBookingManager _accommodationBookingManager;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly ICancellationPoliciesService _cancellationPoliciesService;
        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;

        private readonly IDataProviderClient _dataProviderClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMemoryFlow _flow;
        private readonly ILogger<AccommodationService> _logger;
        private readonly DataProviderOptions _options;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IServiceAccountContext _serviceAccountContext;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly IBookingAuditLogService  _bookingAuditLogService;
    }
}