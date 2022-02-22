using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.SupplierConnectors;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.SupplierResponses
{
    public class NetstormingResponseService
    {
        public NetstormingResponseService(IConnectorClient connectorClient,
            ISupplierOptionsStorage supplierOptionsStorage,
            IBookingResponseProcessor responseProcessor,
            ILogger<NetstormingResponseService> logger)
        {
            _connectorClient = connectorClient;
            _supplierOptionsStorage = supplierOptionsStorage;
            _responseProcessor = responseProcessor;
            _logger = logger;
        }


        public async Task<Result> ProcessBookingDetailsResponse(byte[] xmlRequestData)
        {
            var (_, isGetBookingDetailsFailure, bookingDetails , bookingDetailsError) = await GetBookingDetailsFromConnector(xmlRequestData);
            if (isGetBookingDetailsFailure)
            {
                _logger.LogUnableGetBookingDetailsFromNetstormingXml(Encoding.UTF8.GetString(xmlRequestData));
                return Result.Failure(bookingDetailsError);
            }

            await _responseProcessor.ProcessResponse(bookingDetails, ApiCaller.FromSupplier(NetstormingCode), Common.Enums.BookingChangeEvents.SupplierWebHook);

            return Result.Success();
        }
        

        private async Task<Result<Booking>> GetBookingDetailsFromConnector(byte[] xmlData)
        {
            var requestMessageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post,
                new Uri($"{_supplierOptionsStorage.GetByCode(NetstormingCode).ConnectorUrl}" + "bookings/response"))
            {
                Content = new ByteArrayContent(xmlData)
            });

            var (_, isFailure, bookingDetails, error) = await _connectorClient.Send<Booking>(requestMessageFactory);
            return isFailure 
                ? Result.Failure<Booking>(error.Detail) 
                : Result.Success(bookingDetails);
        }
        
        private const string NetstormingCode = "netstorming";

        private readonly IConnectorClient _connectorClient;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        private readonly IBookingResponseProcessor _responseProcessor;
        private readonly ILogger<NetstormingResponseService> _logger;
    }
}