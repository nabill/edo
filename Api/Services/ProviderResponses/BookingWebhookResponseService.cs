using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class BookingWebhookResponseService: IBookingWebhookResponseService
    {
        public BookingWebhookResponseService(
             IDataProviderFactory dataProviderFactory,
             IBookingManager bookingManager,
             IBookingService bookingService,
             ICustomerContext customerContext)
        {
            _dataProviderFactory = dataProviderFactory;
            _bookingManager = bookingManager;
            _customerContext = customerContext;
            _bookingService = bookingService;
        }
        
        
        public async Task<Result> ProcessBookingData(Stream stream, DataProviders dataProvider)
        {
            if (!AsyncDataProviders.Contains(dataProvider))
                return Result.Fail($"{nameof(dataProvider)} '{dataProvider}' isn't asynchronous." +
                    $"Asynchronous data providers: {string.Join(", ", AsyncDataProviders)}");
            
            var (_, isGettingBookingDetailsFailure, bookingDetails, gettingBookingDetailsError) = await _dataProviderFactory.Get(dataProvider).ProcessAsyncResponse(stream);
            if (isGettingBookingDetailsFailure)
                return Result.Fail(gettingBookingDetailsError.Detail);
            
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingManager.Get(bookingDetails.ReferenceCode);
            
            if (isGetBookingFailure)
                return Result.Fail(getBookingError);
            
            await _customerContext.SetCustomerInfo(booking.CustomerId);
            
            return await _bookingService.ProcessResponse(bookingDetails, booking); 
        }

        private readonly IDataProviderFactory _dataProviderFactory;
        private readonly IBookingManager _bookingManager;
        private readonly ICustomerContext _customerContext;
        private readonly IBookingService _bookingService;
        private static readonly List<DataProviders> AsyncDataProviders = new List<DataProviders>{DataProviders.Netstorming, DataProviders.Etg};
    }
}