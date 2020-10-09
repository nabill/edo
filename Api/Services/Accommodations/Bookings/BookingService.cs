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
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingService : IBookingService
    {
        public BookingService(IBookingEvaluationStorage bookingEvaluationStorage,
            IBookingRecordsManager bookingRecordsManager,
            IBookingAuditLogService bookingAuditLogService,
            ISupplierOrderService supplierOrderService,
            EdoContext context,
            IBookingMailingService bookingMailingService,
            ILogger<BookingService> logger,
            IDataProviderManager dataProviderFactory,
            IBookingDocumentsService documentsService,
            IBookingPaymentService paymentService,
            IPaymentNotificationService notificationService,
            IAccountPaymentService accountPaymentService,
            IDateTimeProvider dateTimeProvider,
            IAvailabilitySearchSettingsService availabilitySearchSettingsService)
        {
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _bookingRecordsManager = bookingRecordsManager;
            _bookingAuditLogService = bookingAuditLogService;
            _supplierOrderService = supplierOrderService;
            _context = context;
            _bookingMailingService = bookingMailingService;
            _logger = logger;
            _dataProviderManager = dataProviderFactory;
            _documentsService = documentsService;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _accountPaymentService = accountPaymentService;
            _dateTimeProvider = dateTimeProvider;
            _availabilitySearchSettingsService = availabilitySearchSettingsService;
        }


        public async Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest, AgentContext agentContext, string languageCode)
        {
            string availabilityId = default;
            var settings = await _availabilitySearchSettingsService.Get(agentContext);

            return await GetCachedAvailability(bookingRequest, agentContext)
                .Ensure(AreAprSettingsSuitable, ProblemDetailsBuilder.Build("You can't book the restricted contract without explicit approval from a Happytravel.com officer."))
                .Ensure(AreDeadlineSettingsSuitable, ProblemDetailsBuilder.Build("You can't book the contract within deadline without explicit approval from a Happytravel.com officer."))
                .Tap(FillAvailabilityId)
                .Map(ExtractBookingAvailabilityInfo)
                .Map(Register)
                .Finally(WriteLog);


            bool AreAprSettingsSuitable(
                (DataProviders, DataWithMarkup<RoomContractSetAvailability>) bookingData)
            {
                var (_, dataWithMarkup) = bookingData;
                if (!dataWithMarkup.Data.RoomContractSet.IsAdvancedPurchaseRate)
                    return true;

                return settings.AprMode switch
                {
                    AprMode.CardAndAccountPurchases => true,
                    AprMode.CardPurchasesOnly
                        when bookingRequest.PaymentMethod == PaymentMethods.CreditCard => true,
                    _ => false
                };
            }
            
            
            bool AreDeadlineSettingsSuitable(
                (DataProviders, DataWithMarkup<RoomContractSetAvailability>) bookingData)
            {
                var (_, dataWithMarkup) = bookingData;
                var deadlineDate = dataWithMarkup.Data.RoomContractSet.Deadline.Date ?? dataWithMarkup.Data.CheckInDate;
                if (deadlineDate.Date >= _dateTimeProvider.UtcTomorrow())
                    return true;

                return settings.PassedDeadlineOffersMode switch
                {
                    PassedDeadlineOffersMode.CardAndAccountPurchases => true,
                    PassedDeadlineOffersMode.CardPurchasesOnly
                        when bookingRequest.PaymentMethod == PaymentMethods.CreditCard => true,
                    _ => false
                };
            }


            void FillAvailabilityId((DataProviders, DataWithMarkup<RoomContractSetAvailability> Result) responseWithMarkup)
                => availabilityId = responseWithMarkup.Result.Data.AvailabilityId;


            async Task<string> Register(BookingAvailabilityInfo bookingAvailability)
            {
                var bookingRequestWithAvailabilityId = new AccommodationBookingRequest(bookingRequest, availabilityId);
                return await _bookingRecordsManager.Register(bookingRequestWithAvailabilityId, bookingAvailability, agentContext, languageCode);
            }


            Result<string, ProblemDetails> WriteLog(Result<string, ProblemDetails> result)
                => WriteLogByResult(result,
                    () => _logger.LogBookingRegistrationSuccess($"Successfully registered a booking with reference code: '{result.Value}'"),
                    () => _logger.LogBookingRegistrationFailure($"Failed to register a booking. AvailabilityId: '{availabilityId}'. " +
                        $"Itinerary number: {bookingRequest.ItineraryNumber}. Passenger name: {bookingRequest.MainPassengerName}. Error: {result.Error.Detail}"))
            ;
        }


        public async Task<Result<AccommodationBookingInfo, ProblemDetails>> Finalize(string referenceCode, AgentContext agentContext, string languageCode)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await GetAgentsBooking()
                .Ensure(b => agentContext.IsUsingAgency(b.AgencyId), ProblemDetailsBuilder.Build("The booking does not belong to your current agency"))
                .Bind(CheckBookingIsPaid)
                .OnFailure(WriteLogFailure);

            if (isGetBookingFailure)
                return Result.Failure<AccommodationBookingInfo, ProblemDetails>(getBookingError);

            return await BookOnProvider(booking, referenceCode, languageCode)
                .Tap(ProcessResponse)
                .OnFailure(VoidMoneyAndCancelBooking)
                .Bind(GenerateInvoice)
                .Bind(SendReceipt)
                .Bind(GetAccommodationBookingInfo)
                .Tap(NotifyBookingFinalized)
                .Tap(SendInvoice)
                .Finally(WriteLog);


            Task<Result<Data.Booking.Booking, ProblemDetails>> GetAgentsBooking()
                => _bookingRecordsManager.GetAgentsBooking(referenceCode, agentContext).ToResultWithProblemDetails();


            Result<Data.Booking.Booking, ProblemDetails> CheckBookingIsPaid(Data.Booking.Booking bookingFromPipe)
            {
                if (bookingFromPipe.PaymentStatus == BookingPaymentStatuses.NotPaid)
                {
                    _logger.LogBookingFinalizationPaymentFailure($"The booking with reference code: '{referenceCode}' hasn't been paid");
                    return ProblemDetailsBuilder.Fail<Data.Booking.Booking>("The booking hasn't been paid");
                }

                return bookingFromPipe;
            }


            Task ProcessResponse(Booking bookingResponse) => this.ProcessResponse(bookingResponse, booking);


            Task VoidMoneyAndCancelBooking(ProblemDetails problemDetails) => this.VoidMoneyAndCancelBooking(booking, agentContext);


            Task<Result<Booking, ProblemDetails>> SendReceipt(Booking details) => this.SendReceipt(details, booking, agentContext);


            Task<Result<Booking, ProblemDetails>> GenerateInvoice(Booking details) => this.GenerateInvoice(details, referenceCode, agentContext);


            Task<Result<AccommodationBookingInfo, ProblemDetails>> GetAccommodationBookingInfo(Booking details)
                => _bookingRecordsManager.GetAgentAccommodationBookingInfo(details.ReferenceCode, agentContext, languageCode)
                    .ToResultWithProblemDetails();


            Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo) => _bookingMailingService.NotifyBookingFinalized(bookingInfo, agentContext);
            
            
            Task SendInvoice(AccommodationBookingInfo bookingInfo) => _bookingMailingService.SendInvoice(bookingInfo.BookingId, agentContext.Email, agentContext, languageCode);


            void WriteLogFailure(ProblemDetails problemDetails)
                => _logger.LogBookingByAccountFailure($"Failed to finalize a booking with reference code: '{referenceCode}'. Error: {problemDetails.Detail}");


            Result<T, ProblemDetails> WriteLog<T>(Result<T, ProblemDetails> result)
                => WriteLogByResult(result,
                    () => _logger.LogBookingFinalizationSuccess($"Successfully finalized a booking with reference code: '{referenceCode}'"),
                    () => _logger.LogBookingFinalizationFailure(
                        $"Failed to finalize a booking with reference code: '{referenceCode}'. Error: {result.Error.Detail}"));
        }


        public async Task<Result<AccommodationBookingInfo, ProblemDetails>> BookByAccount(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp)
        {
            string availabilityId = default;
            DateTime? availabilityDeadline = default;
            string referenceCode = default;
            var wasPaymentMade = false;

            var (_, isRegisterFailure, booking, registerError) = await GetCachedAvailability(bookingRequest, agentContext)
                .Tap(FillAvailabilityLocalVariables)
                .Map(ExtractBookingAvailabilityInfo)
                .BindWithTransaction(_context, info => Result.Success<BookingAvailabilityInfo, ProblemDetails>(info)
                    .Map(RegisterBooking)
                    .Bind(GetBooking)
                    .Bind(PayUsingAccountIfDeadlinePassed))
                .OnFailure(WriteLogFailure);

            if (isRegisterFailure)
                return Result.Failure<AccommodationBookingInfo, ProblemDetails>(registerError);

            return await BookOnProvider(booking, booking.ReferenceCode, languageCode)
                .Tap(ProcessResponse)
                .OnFailure(VoidMoneyAndCancelBooking)
                .Bind(GenerateInvoice)
                .Bind(SendReceiptIfPaymentMade)
                .Bind(GetAccommodationBookingInfo)
                .Tap(NotifyBookingFinalized)
                .Tap(SendInvoice)
                .Finally(WriteLog);


            void FillAvailabilityLocalVariables((DataProviders, DataWithMarkup<RoomContractSetAvailability> Result) responseWithMarkup)
            {
                availabilityId = responseWithMarkup.Result.Data.AvailabilityId;
                availabilityDeadline = responseWithMarkup.Result.Data.RoomContractSet.Deadline.Date;
            }


            async Task<string> RegisterBooking(BookingAvailabilityInfo bookingAvailability)
            {
                var bookingRequestWithAvailabilityId = new AccommodationBookingRequest(bookingRequest, availabilityId);
                var registeredReferenceCode =
                    await _bookingRecordsManager.Register(bookingRequestWithAvailabilityId, bookingAvailability, agentContext, languageCode);

                referenceCode = registeredReferenceCode;
                return registeredReferenceCode;
            }


            async Task<Result<Data.Booking.Booking, ProblemDetails>> GetBooking(string referenceCode)
                => await _bookingRecordsManager.Get(referenceCode).ToResultWithProblemDetails();


            async Task<Result<Data.Booking.Booking, ProblemDetails>> PayUsingAccountIfDeadlinePassed(Data.Booking.Booking bookingInPipeline)
            {
                if (!availabilityDeadline.HasValue || availabilityDeadline > _dateTimeProvider.UtcNow())
                    return bookingInPipeline;

                var (_, isPaymentFailure, _, paymentError) = await _accountPaymentService.Charge(bookingInPipeline, agentContext, clientIp);
                if (isPaymentFailure)
                    return ProblemDetailsBuilder.Fail<Data.Booking.Booking>(paymentError);

                wasPaymentMade = true;
                return bookingInPipeline;
            }


            Task ProcessResponse(Booking bookingResponse) => this.ProcessResponse(bookingResponse, booking);

            Task VoidMoneyAndCancelBooking(ProblemDetails problemDetails) => this.VoidMoneyAndCancelBooking(booking, agentContext);

            Task<Result<Booking, ProblemDetails>> GenerateInvoice(Booking details) => this.GenerateInvoice(details, booking.ReferenceCode, agentContext);


            async Task<Result<Booking, ProblemDetails>> SendReceiptIfPaymentMade(Booking details)
                => wasPaymentMade
                    ? await SendReceipt(details, booking, agentContext)
                    : details;


            Task<Result<AccommodationBookingInfo, ProblemDetails>> GetAccommodationBookingInfo(Booking details)
                => _bookingRecordsManager.GetAgentAccommodationBookingInfo(details.ReferenceCode, agentContext, languageCode)
                    .ToResultWithProblemDetails();
            

            Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo) => _bookingMailingService.NotifyBookingFinalized(bookingInfo, agentContext);

            
            Task SendInvoice(AccommodationBookingInfo bookingInfo) => _bookingMailingService.SendInvoice(bookingInfo.BookingId, agentContext.Email, agentContext, languageCode);
            
            
            void WriteLogFailure(ProblemDetails problemDetails)
                => _logger.LogBookingByAccountFailure($"Failed to book using account. Reference code: '{referenceCode}'. Error: {problemDetails.Detail}");


            Result<T, ProblemDetails> WriteLog<T>(Result<T, ProblemDetails> result)
                => WriteLogByResult(result,
                    () => _logger.LogBookingFinalizationSuccess($"Successfully booked using account. Reference code: '{referenceCode}'"),
                    () => _logger.LogBookingFinalizationFailure(
                        $"Failed to book using account. Reference code: '{referenceCode}'. Error: {result.Error.Detail}"));
        }


        public async Task ProcessResponse(Booking bookingResponse, Data.Booking.Booking booking)
        {
            await _bookingAuditLogService.Add(bookingResponse, booking);

            _logger.LogBookingResponseProcessStarted(
                $"Start the booking response processing with the reference code '{bookingResponse.ReferenceCode}'. Old status: {booking.Status}");

            if (bookingResponse.Status == BookingStatusCodes.NotFound)
            {
                await ProcessBookingNotFound();
                return;
            }

            if (bookingResponse.Status.ToInternalStatus() == booking.Status)
            {
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has been successfully processed. No changes applied");
                return;
            }
                

            await UpdateBookingDetails();

            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Confirmed:
                    await ConfirmBooking();
                    break;
                case BookingStatusCodes.Cancelled:
                    await ProcessBookingCancellation(booking, UserInfo.InternalServiceAccount);
                    break;
            }

            _logger.LogBookingResponseProcessSuccess(
                $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has been successfully processed. " +
                $"New status: {bookingResponse.Status}");


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


            async Task ProcessBookingNotFound()
            {
                if (_dateTimeProvider.UtcNow() < booking.Created + BookingCheckTimeout)
                {
                    _logger.LogBookingResponseProcessSuccess(
                        $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has not been processed due to '{BookingStatusCodes.NotFound}' status.");
                }
                else
                {
                    await _bookingRecordsManager.SetNeedsManualCorrectionStatus(booking);
                    _logger.LogBookingResponseProcessSuccess(
                        $"The booking response with the reference code '{bookingResponse.ReferenceCode}' set as needed manual processing.");
                }
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
                return Result.Success();
            }
            */
        }


        public async Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, AgentContext agent)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingId, agent.AgentId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(getBookingError);

            if (!agent.IsUsingAgency(booking.AgencyId))
                return ProblemDetailsBuilder.Fail<VoidObject>("The booking does not belong to your current agency");

            return await CancelBooking(booking, agent.ToUserInfo());
        }


        public async Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, ServiceAccount serviceAccount)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(getBookingError);

            return await CancelBooking(booking, serviceAccount.ToUserInfo());
        }


        public async Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, Administrator administrator, bool requireProviderConfirmation)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isGetBookingFailure)
                return ProblemDetailsBuilder.Fail<VoidObject>(getBookingError);

            return await CancelBooking(booking, administrator.ToUserInfo(), requireProviderConfirmation);
        }


        public async Task<Result<Booking, ProblemDetails>> RefreshStatus(int bookingId)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingId);
            if (isGetBookingFailure)
            {
                _logger.LogBookingRefreshStatusFailure(
                    $"Failed to refresh status for a booking with id {bookingId} while getting the booking. Error: {getBookingError}");
                return ProblemDetailsBuilder.Fail<Booking>(getBookingError);
            }

            var oldStatus = booking.Status;
            var referenceCode = booking.ReferenceCode;
            var (_, isGetDetailsFailure, newDetails, getDetailsError) = await _dataProviderManager
                .Get(booking.DataProvider)
                .GetBookingDetails(referenceCode, booking.LanguageCode);

            if (isGetDetailsFailure)
            {
                _logger.LogBookingRefreshStatusFailure($"Failed to refresh status for a booking with reference code: '{referenceCode}' " +
                    $"while getting info from a provider. Error: {getBookingError}");
                return Result.Failure<Booking, ProblemDetails>(getDetailsError);
            }

            await ProcessResponse(newDetails, booking);

            _logger.LogBookingRefreshStatusSuccess($"Successfully refreshed status fot a booking with reference code: '{referenceCode}'. " +
                $"Old status: {oldStatus}. New status: {newDetails.Status}");

            return Result.Success<Booking, ProblemDetails>(newDetails);
        }


        private async Task<Result<(DataProviders, DataWithMarkup<RoomContractSetAvailability>), ProblemDetails>> GetCachedAvailability(
            AccommodationBookingRequest bookingRequest, AgentContext agentContext)
            => await _bookingEvaluationStorage.Get(bookingRequest.SearchId,
                    bookingRequest.ResultId,
                    bookingRequest.RoomContractSetId,
                    (await _availabilitySearchSettingsService.Get(agentContext)).EnabledConnectors)
                .ToResultWithProblemDetails();


        private BookingAvailabilityInfo ExtractBookingAvailabilityInfo(
            (DataProviders Source, DataWithMarkup<RoomContractSetAvailability> Result) responseWithMarkup)
            => ExtractBookingAvailabilityInfo(responseWithMarkup.Source, responseWithMarkup.Result.Data);
        // Temporarily saving availability id along with booking request to get it on the booking step.
        // TODO NIJO-813: Rewrite this to save such data in another place


        private async Task<Result<Booking, ProblemDetails>> BookOnProvider(Data.Booking.Booking booking, string referenceCode, string languageCode)
        {
            // TODO: will be implemented in NIJO-31 
            var bookingRequest = JsonConvert.DeserializeObject<AccommodationBookingRequest>(booking.BookingRequest);

            var features = new List<Feature>(); //bookingRequest.Features

            var roomDetails = bookingRequest.RoomDetails
                .Select(d => new SlimRoomOccupation(d.Type, d.Passengers, d.IsExtraBedNeeded))
                .ToList();

            var innerRequest = new BookingRequest(bookingRequest.AvailabilityId,
                bookingRequest.RoomContractSetId,
                booking.ReferenceCode,
                roomDetails,
                features,
                bookingRequest.RejectIfUnavailable);

            try
            {
                var bookingResult = await _dataProviderManager
                    .Get(booking.DataProvider)
                    .Book(innerRequest, languageCode);

                if (bookingResult.IsSuccess)
                {
                    return bookingResult.Value;
                }

                // If result is failed this does not mean that booking failed. This means that we should check it later.
                _logger.LogBookingFinalizationFailure($"The booking finalization with the reference code: '{referenceCode}' has been failed");
                return GetStubDetails(booking);
            }
            catch
            {
                var errorMessage = $"Failed to update booking data (refcode '{referenceCode}') after the request to the connector";

                var (_, isCancellationFailed, cancellationError) = await _dataProviderManager.Get(booking.DataProvider).CancelBooking(booking.ReferenceCode);
                if (isCancellationFailed)
                    errorMessage += Environment.NewLine + $"Booking cancellation has failed: {cancellationError}";

                _logger.LogBookingFinalizationFailure(errorMessage);

                return GetStubDetails(booking);
            }


            // TODO: Remove room information and contract description from booking NIJO-915
            static Booking GetStubDetails(Data.Booking.Booking booking)
                => new Booking(booking.ReferenceCode,
                    // Will be set in the refresh step
                    string.Empty,
                    BookingStatusCodes.WaitingForResponse,
                    booking.AccommodationId,
                    booking.SupplierReferenceCode,
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    // Remove during NIJO-915
                    string.Empty,
                    booking.DeadlineDate,
                    // Remove during NIJO-915
                    new List<SlimRoomOccupationWithPrice>(0),
                    BookingUpdateMode.Asynchronous,
                    new RoomContractSet(Guid.Empty, 
                        default, default, new List<RoomContract>()));
        }


        private async Task VoidMoneyAndCancelBooking(Data.Booking.Booking booking, AgentContext agentContext)
        {
            var (_, isFailure, _, error) = await _dataProviderManager.Get(booking.DataProvider).CancelBooking(booking.ReferenceCode);
            if (isFailure)
            {
                _logger.LogBookingCancelFailure(
                    $"Failed to cancel a booking with reference code '{booking.ReferenceCode}': [{error.Status}] {error.Detail}");

                // We'll refund money only if the booking cancellation was succeeded on supplier
                return;
            }

            var (_, voidOrRefundFailure, voidOrRefundError) = await _paymentService.VoidOrRefund(booking, agentContext.ToUserInfo());
            if (voidOrRefundFailure)
                _logger.LogBookingCancelFailure($"Failure during cancellation of a booking with reference code '{booking.ReferenceCode}':" +
                    $"failed to void or refund money. Error: {voidOrRefundError}");
        }


        private async Task<Result<Booking, ProblemDetails>> SendReceipt(Booking details, Data.Booking.Booking booking, AgentContext agentContext)
        {
            var (_, isReceiptFailure, receiptInfo, receiptError) = await _documentsService.GenerateReceipt(booking.Id, agentContext.AgentId);
            if (isReceiptFailure)
                return ProblemDetailsBuilder.Fail<Booking>(receiptError);

            await _notificationService.SendReceiptToCustomer(receiptInfo, agentContext.Email);
            return Result.Success<Booking, ProblemDetails>(details);
        }


        private async Task<Result<Booking, ProblemDetails>> GenerateInvoice(Booking details, string referenceCode, AgentContext agent)
        {
            var (_, isInvoiceFailure, invoiceError) = await _documentsService.GenerateInvoice(referenceCode, agent);
            if (isInvoiceFailure)
                return ProblemDetailsBuilder.Fail<Booking>(invoiceError);

            return Result.Success<Booking, ProblemDetails>(details);
        }


        private async Task<Result<VoidObject, ProblemDetails>> CancelBooking(Data.Booking.Booking booking, UserInfo user,
            bool requireProviderConfirmation = true)
        {
            if (booking.Status == BookingStatuses.Cancelled)
            {
                _logger.LogBookingAlreadyCancelled(
                    $"Skipping cancellation for a booking with reference code: '{booking.ReferenceCode}'. Already cancelled.");
                return Result.Success<VoidObject, ProblemDetails>(VoidObject.Instance);
            }

            return await SendCancellationRequest()
                .Bind(ProcessCancellation)
                .Finally(WriteLog);


            async Task<Result<Data.Booking.Booking, ProblemDetails>> SendCancellationRequest()
            {
                var (_, isCancelFailure, _, cancelError) = await _dataProviderManager.Get(booking.DataProvider).CancelBooking(booking.ReferenceCode);
                return isCancelFailure && requireProviderConfirmation
                    ? Result.Failure<Data.Booking.Booking, ProblemDetails>(cancelError)
                    : Result.Success<Data.Booking.Booking, ProblemDetails>(booking);
            }

            
            async Task<Result<VoidObject, ProblemDetails>> ProcessCancellation(Data.Booking.Booking b)
            {
                if(b.UpdateMode == BookingUpdateMode.Synchronous || !requireProviderConfirmation)
                    return await ProcessBookingCancellation(b, user).ToResultWithProblemDetails();

                return VoidObject.Instance;
            }


            Result<T, ProblemDetails> WriteLog<T>(Result<T, ProblemDetails> result)
                => WriteLogByResult(result,
                    () => _logger.LogBookingCancelSuccess($"Successfully cancelled a booking with reference code: '{booking.ReferenceCode}'"),
                    () => _logger.LogBookingCancelFailure(
                        $"Failed to cancel a booking with reference code: '{booking.ReferenceCode}'. Error: {result.Error.Detail}"));
        }


        private Task<Result> ProcessBookingCancellation(Data.Booking.Booking booking, UserInfo user)
        {
            return SendNotifications()
                .Tap(CancelSupplierOrder)
                .Bind(VoidMoney)
                .Tap(SetBookingCancelled);

            
            async Task CancelSupplierOrder()
            {
                var referenceCode = booking.ReferenceCode;
                await _supplierOrderService.Cancel(referenceCode);
            }


            async Task<Result> SendNotifications()
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                if (agent == default)
                {
                    _logger.LogWarning("Booking cancellation notification: could not find agent with id '{0}' for the booking '{1}'",
                        booking.AgentId, booking.ReferenceCode);
                    return Result.Success();
                }

                await _bookingMailingService.NotifyBookingCancelled(booking.ReferenceCode, agent.Email, $"{agent.LastName} {agent.FirstName}");
                return Result.Success();
            }

            async Task<Result> VoidMoney()
            {
                if (booking.PaymentStatus == BookingPaymentStatuses.Authorized || booking.PaymentStatus == BookingPaymentStatuses.Captured)
                    return await _paymentService.VoidOrRefund(booking, user);

                return Result.Success();
            }


            Task SetBookingCancelled() => _bookingRecordsManager.ConfirmBookingCancellation(booking);
        }


        private BookingAvailabilityInfo ExtractBookingAvailabilityInfo(DataProviders dataProvider, RoomContractSetAvailability response)
        {
            var location = response.Accommodation.Location;

            return new BookingAvailabilityInfo(
                response.Accommodation.Id,
                response.Accommodation.Name,
                response.RoomContractSet,
                location.LocalityZone,
                location.Locality,
                location.Country,
                location.CountryCode,
                location.Address,
                location.Coordinates,
                response.CheckInDate,
                response.CheckOutDate,
                response.NumberOfNights,
                dataProvider);
        }


        private Result<T, ProblemDetails> WriteLogByResult<T>(Result<T, ProblemDetails> result, Action logSuccess, Action logFailure)
        {
            if (result.IsSuccess)
                logSuccess();
            else
                logFailure();

            return result;
        }

        private static readonly TimeSpan BookingCheckTimeout = TimeSpan.FromMinutes(30);

        private readonly IAccountPaymentService _accountPaymentService;
        private readonly IBookingAuditLogService _bookingAuditLogService;


        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly EdoContext _context;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAvailabilitySearchSettingsService _availabilitySearchSettingsService;
        private readonly IBookingDocumentsService _documentsService;
        private readonly ILogger<BookingService> _logger;
        private readonly IPaymentNotificationService _notificationService;
        private readonly IBookingPaymentService _paymentService;
        private readonly ISupplierOrderService _supplierOrderService;
    }
}