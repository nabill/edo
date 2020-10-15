using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class BookingWebhookResponseService: IBookingWebhookResponseService
    {
        public BookingWebhookResponseService(
             IDataProviderManager dataProviderManager,
             IBookingRecordsManager bookingRecordsManager,
             BookingResponseProcessor responseProcessor)
        {
            _dataProviderManager = dataProviderManager;
            _bookingRecordsManager = bookingRecordsManager;
            _responseProcessor = responseProcessor;
        }
        
        
        public async Task<Result> ProcessBookingData(Stream stream, DataProviders dataProvider)
        {
            if (!AsyncDataProviders.Contains(dataProvider))
                return Result.Failure($"{nameof(dataProvider)} '{dataProvider}' isn't asynchronous." +
                    $"Asynchronous data providers: {string.Join(", ", AsyncDataProviders)}");
            
            var (_, isGettingBookingDetailsFailure, bookingDetails, gettingBookingDetailsError) = await _dataProviderManager.Get(dataProvider).ProcessAsyncResponse(stream);
            if (isGettingBookingDetailsFailure)
                return Result.Failure(gettingBookingDetailsError.Detail);
            
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingRecordsManager.Get(bookingDetails.ReferenceCode);
            
            if (isGetBookingFailure)
                return Result.Failure(getBookingError);
            
            await _responseProcessor.ProcessResponse(bookingDetails, booking);
            return Result.Success();
        }

        private readonly IDataProviderManager _dataProviderManager;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly BookingResponseProcessor _responseProcessor;
        private static readonly List<DataProviders> AsyncDataProviders = new List<DataProviders>{DataProviders.Netstorming, DataProviders.Etg};
    }
}