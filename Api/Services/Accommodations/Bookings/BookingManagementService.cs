using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingManagementService : IBookingManagementService
    {
        public BookingManagementService(IBookingRecordsManager bookingRecordsManager,
            ILogger<BookingManagementService> logger,
            IDataProviderManager dataProviderFactory,
            BookingChangesProcessor bookingChangesProcessor,
            BookingResponseProcessor responseProcessor)
        {
            _bookingRecordsManager = bookingRecordsManager;
            _logger = logger;
            _dataProviderManager = dataProviderFactory;
            _bookingChangesProcessor = bookingChangesProcessor;
            _responseProcessor = responseProcessor;
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

            await _responseProcessor.ProcessResponse(newDetails, booking);

            _logger.LogBookingRefreshStatusSuccess($"Successfully refreshed status fot a booking with reference code: '{referenceCode}'. " +
                $"Old status: {oldStatus}. New status: {newDetails.Status}");

            return Result.Success<Booking, ProblemDetails>(newDetails);
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
                    return await _bookingChangesProcessor.ProcessCancellation(b, user).ToResultWithProblemDetails();

                return VoidObject.Instance;
            }


            Result<T, ProblemDetails> WriteLog<T>(Result<T, ProblemDetails> result)
                => LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogBookingCancelSuccess($"Successfully cancelled a booking with reference code: '{booking.ReferenceCode}'"),
                    () => _logger.LogBookingCancelFailure(
                        $"Failed to cancel a booking with reference code: '{booking.ReferenceCode}'. Error: {result.Error.Detail}"));
        }


        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly BookingChangesProcessor _bookingChangesProcessor;
        private readonly BookingResponseProcessor _responseProcessor;
        private readonly ILogger<BookingManagementService> _logger;
    }
}