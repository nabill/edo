using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class WebhookResponseService
    {
        public WebhookResponseService(
             ISupplierConnectorManager supplierConnectorManager,
             IBookingRecordsManager bookingRecordsManager,
             IBookingResponseProcessor responseProcessor)
        {
            _supplierConnectorManager = supplierConnectorManager;
            _bookingRecordsManager = bookingRecordsManager;
            _responseProcessor = responseProcessor;
        }
        
        
        public async Task<Result> ProcessBookingData(Stream stream, Suppliers supplier)
        {
            var (_, isGettingBookingDetailsFailure, bookingDetails, gettingBookingDetailsError) = await _supplierConnectorManager.Get(supplier).ProcessAsyncResponse(stream);
            if (isGettingBookingDetailsFailure)
                return Result.Failure(gettingBookingDetailsError.Detail);
            
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingDetails.ReferenceCode);
            
            if (isGetBookingFailure)
                return Result.Failure(getBookingError);
            
            await _responseProcessor.ProcessResponse(bookingDetails, booking);
            return Result.Success();
        }

        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingResponseProcessor _responseProcessor;
    }
}