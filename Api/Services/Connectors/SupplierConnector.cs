using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Metrics;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class SupplierConnector : ISupplierConnector
    {
        public SupplierConnector(string supplierName, IConnectorClient connectorClient, string baseUrl,
            ILogger<SupplierConnector> logger)
        {
            _supplierName = supplierName;
            _connectorClient = connectorClient;
            _baseUrl = baseUrl;
            _logger = logger;
        }
        
        
        public Task<Result<Availability, ProblemDetails>> GetAvailability(AvailabilityRequest request, string languageCode)
        {
            return ExecuteWithLogging(Counters.WideAvailabilitySearch, () =>
            {
                return _connectorClient.Post<AvailabilityRequest, Availability>(
                    new Uri(_baseUrl + "accommodations/availabilities", UriKind.Absolute), request, languageCode);
            });
        }


        public Task<Result<AccommodationAvailability, ProblemDetails>> GetAvailability(string availabilityId,
            string accommodationId, string languageCode)
        {
            return ExecuteWithLogging(Counters.RoomSelection, () =>
            {
                return _connectorClient.Post<AccommodationAvailability>(
                    new Uri(_baseUrl + "accommodations/" + accommodationId + "/availabilities/" + availabilityId, UriKind.Absolute), languageCode);
            });
        }
        
        
        public Task<Result<RoomContractSetAvailability?, ProblemDetails>> GetExactAvailability(string availabilityId, Guid roomContractSetId, string languageCode)
        {
            return ExecuteWithLogging(Counters.Evaluation, () =>
            {
                return _connectorClient.Post<RoomContractSetAvailability?>(
                    new Uri($"{_baseUrl}accommodations/availabilities/{availabilityId}/room-contract-sets/{roomContractSetId}", UriKind.Absolute), languageCode);
            });
        }


        public Task<Result<Deadline, ProblemDetails>> GetDeadline(string availabilityId, Guid roomContractSetId, string languageCode)
        {
            return ExecuteWithLogging(Counters.BookingDeadline, () =>
            {
                var uri = new Uri($"{_baseUrl}accommodations/availabilities/{availabilityId}/room-contract-sets/{roomContractSetId}/deadline", UriKind.Absolute);
                return _connectorClient.Get<Deadline>(uri, languageCode);
            });
        }


        public Task<Result<Booking, ProblemDetails>> Book(BookingRequest request, string languageCode)
        {
            return ExecuteWithLogging(Counters.Booking, () =>
            {
                return _connectorClient.Post<BookingRequest, Booking>(
                    new Uri(_baseUrl + "accommodations/bookings", UriKind.Absolute),
                    request, languageCode);
            });
        }


        public Task<Result<Unit, ProblemDetails>> CancelBooking(string referenceCode)
        {
            return ExecuteWithLogging(Counters.Cancellation, () =>
            {
                return _connectorClient.Post(new Uri(_baseUrl + "accommodations/bookings/" + referenceCode + "/cancel",
                    UriKind.Absolute));
            });
        }


        public Task<Result<Booking, ProblemDetails>> GetBookingDetails(string referenceCode, string languageCode)
        {
            return ExecuteWithLogging(Counters.BookingInformation, () =>
            {
                return _connectorClient.Get<Booking>(
                    new Uri(_baseUrl + "accommodations/bookings/" + referenceCode,
                        UriKind.Absolute), languageCode);
            });
        }


        public Task<Result<Booking, ProblemDetails>> ProcessAsyncResponse(Stream stream)
        {
            return ExecuteWithLogging(Counters.BookingAsyncResponse, () =>
            {
                return _connectorClient.Post<Booking>(new Uri(_baseUrl + "bookings/response", UriKind.Absolute), stream);
            });
        }
        

        private async Task<Result<TResult, ProblemDetails>> ExecuteWithLogging<TResult>(string step, Func<Task<Result<TResult, ProblemDetails>>> funcToExecute)
        {
            _logger.LogSupplierConnectorRequestStarted(_baseUrl, step);
            
            using var timer = Counters.SupplierRequestHistogram.WithLabels(step, _supplierName).NewTimer();
            var result = await funcToExecute();
            timer.Dispose();
            
            Counters.SupplierRequestCounter
                .WithLabels(step,
                    _supplierName,
                    result.IsFailure ? result.Error.Status.ToString() : string.Empty)
                .Inc();

            LoggerUtils.WriteLogByResult(result,
                () => _logger.LogSupplierConnectorRequestSuccess(_baseUrl, step),
                () => _logger.LogSupplierConnectorRequestError(_baseUrl, result.Error.Detail, step, result.Error.Status));

            return result;
        }


        private readonly string _supplierName;
        private readonly IConnectorClient _connectorClient;
        private readonly string _baseUrl;
        private readonly ILogger<SupplierConnector> _logger;
    }
}