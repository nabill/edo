using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class NetstormingResponseService
    {
        public NetstormingResponseService(
            IConnectorClient connectorClient,
            IDistributedFlow flow,
            IBookingRecordsManager bookingRecordsManager,
            IBookingResponseProcessor responseProcessor,
            IOptions<SupplierOptions> dataProviderOptions,
            ILogger<NetstormingResponseService> logger)
        {
            _connectorClient = connectorClient;
            _supplierOptions = dataProviderOptions.Value;
            _flow = flow;
            _bookingRecordsManager = bookingRecordsManager;
            _responseProcessor = responseProcessor;
            _logger = logger;
        }


        public async Task<Result> ProcessBookingDetailsResponse(byte[] xmlRequestData)
        {
            var (_, isGetBookingDetailsFailure, bookingDetails , bookingDetailsError) = await GetBookingDetailsFromConnector(xmlRequestData);
            if (isGetBookingDetailsFailure)
            {
                _logger.LogUnableGetBookingDetailsFromNetstormingXml("Failed to get booking details from the Netstorming xml:" + 
                    Environment.NewLine + 
                    Encoding.UTF8.GetString(xmlRequestData));
                return Result.Failure(bookingDetailsError);
            }

            var (_, isAcceptFailure, reason) = await AcceptBooking(bookingDetails);
            if (isAcceptFailure)
                return Result.Success(reason);

            var (_, isGetBookingFailure, booking, getBookingError) =
                await _bookingRecordsManager.Get(bookingDetails.ReferenceCode);

            if (isGetBookingFailure)
            {
                _logger.LogWarning(getBookingError);
                return Result.Failure(getBookingError);
            }
            
            _logger.LogUnableGetBookingDetailsFromNetstormingXml($"Set {nameof(booking.AgentId)} to '{booking.AgentId}'");
            
            await _responseProcessor.ProcessResponse(bookingDetails, booking);
            return Result.Success();
        }
        

        private async Task<Result<Booking>> GetBookingDetailsFromConnector(byte[] xmlData)
        {
            var requestMessageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post,
                new Uri($"{_supplierOptions.Netstorming}" + "bookings/response"))
            {
                Content = new ByteArrayContent(xmlData)
            });

            var (_, isFailure, bookingDetails, error) = await _connectorClient.Send<Booking>(requestMessageFactory);
            return isFailure 
                ? Result.Failure<Booking>(error.Detail) 
                : Result.Success(bookingDetails);
        }
        
        
        private async Task<Result> AcceptBooking(Booking bookingDetails)
        {
            if (await IsBookingAccepted(bookingDetails.ReferenceCode, bookingDetails.Status))
            {
                var message = "Booking response has already been accepted:" +
                    $"{nameof(bookingDetails.ReferenceCode)} '{bookingDetails.ReferenceCode}'; " +
                    $"{nameof(bookingDetails.Status)} '{bookingDetails.Status}'";
                
                _logger.LogUnableToAcceptNetstormingRequest(message);
                return Result.Failure(message);
            }

            await _flow.SetAsync(_flow.BuildKey(CacheKeyPrefix, bookingDetails.ReferenceCode, bookingDetails.Status.ToString()), bookingDetails.ReferenceCode, CacheExpirationPeriod);
            
            return Result.Success();
        }


        private async Task<bool> IsBookingAccepted(string  bookingReferenceCode, BookingStatusCodes status)
        {
            var key = _flow.BuildKey(CacheKeyPrefix, bookingReferenceCode, status.ToString());
            var acceptedReferenceCode = await _flow.GetAsync<string>(key);
            return !string.IsNullOrWhiteSpace(acceptedReferenceCode);
        }


        private readonly IConnectorClient _connectorClient;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingResponseProcessor _responseProcessor;
        private readonly SupplierOptions _supplierOptions;
        private readonly IDistributedFlow _flow;
        private readonly ILogger<NetstormingResponseService> _logger;
        
        private static readonly TimeSpan CacheExpirationPeriod = TimeSpan.FromMinutes(2);
        private const string CacheKeyPrefix = "NetstormingResponse";
        
    }
}