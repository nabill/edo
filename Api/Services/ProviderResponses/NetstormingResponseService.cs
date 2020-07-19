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
    public class NetstormingResponseService : INetstormingResponseService
    {
        public NetstormingResponseService(
            IConnectorClient connectorClient,
            IMemoryFlow memoryFlow,
            IBookingRecordsManager bookingRecordsManager,
            IBookingService bookingService,
            IOptions<DataProviderOptions> dataProviderOptions,
            ILogger<NetstormingResponseService> logger)
        {
            _connectorClient = connectorClient;
            _dataProviderOptions = dataProviderOptions.Value;
            _memoryFlow = memoryFlow;
            _bookingRecordsManager = bookingRecordsManager;
            _bookingService = bookingService;
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

            var (_, isAcceptFailure, reason) = AcceptBooking(bookingDetails);
            if (isAcceptFailure)
                return Result.Ok(reason);

            var (_, isGetBookingFailure, booking, getBookingError) =
                await _bookingRecordsManager.Get(bookingDetails.ReferenceCode);

            if (isGetBookingFailure)
            {
                _logger.LogWarning(getBookingError);
                return Result.Failure(getBookingError);
            }
            
            _logger.LogUnableGetBookingDetailsFromNetstormingXml($"Set {nameof(booking.AgentId)} to '{booking.AgentId}'");
            
            await _bookingService.ProcessResponse(bookingDetails, booking);
            return Result.Ok();
        }
        

        private async Task<Result<BookingDetails>> GetBookingDetailsFromConnector(byte[] xmlData)
        {
            var requestMessageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post,
                new Uri($"{_dataProviderOptions.Netstorming}" + "bookings/response"))
            {
                Content = new ByteArrayContent(xmlData)
            });

            var (_, isFailure, bookingDetails, error) = await _connectorClient.Send<BookingDetails>(requestMessageFactory);
            return isFailure 
                ? Result.Failure<BookingDetails>(error.Detail) 
                : Result.Ok(bookingDetails);
        }
        
        
        private Result AcceptBooking(BookingDetails bookingDetails)
        {
            if (IsBookingAccepted(bookingDetails.ReferenceCode, bookingDetails.Status))
            {
                var message = "Booking response has already been accepted:" +
                    $"{nameof(bookingDetails.ReferenceCode)} '{bookingDetails.ReferenceCode}'; " +
                    $"{nameof(bookingDetails.Status)} '{bookingDetails.Status}'";
                
                _logger.LogUnableToAcceptNetstormingRequest(message);
                return Result.Failure(message);
            }

            _memoryFlow.Set(_memoryFlow.BuildKey(CacheKeyPrefix, bookingDetails.ReferenceCode, bookingDetails.Status.ToString()), bookingDetails.ReferenceCode, CacheExpirationPeriod);
            
            return Result.Ok();
        }


        private bool IsBookingAccepted(string  bookingReferenceCode, BookingStatusCodes status) 
            => _memoryFlow.TryGetValue<string>(_memoryFlow.BuildKey(CacheKeyPrefix, bookingReferenceCode, status.ToString()), out _);

        
        private readonly IConnectorClient _connectorClient;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingService _bookingService;
        private readonly DataProviderOptions _dataProviderOptions;
        private readonly IMemoryFlow _memoryFlow;
        private readonly ILogger<NetstormingResponseService> _logger;
        
        private static readonly TimeSpan CacheExpirationPeriod = TimeSpan.FromMinutes(2);
        private const string CacheKeyPrefix = "NetstormingResponse";
        
    }
}