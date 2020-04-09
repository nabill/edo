using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Webhooks.Etg;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.ProviderResponses
{
    public class EtgResponseService: IEtgResponseService
    {
        public EtgResponseService(IOptions<EtgOptions> etgOptions, IOptions<DataProviderOptions> dataProviderOptions, IDataProviderFactory dataProviderFactory, IBookingManager bookingManager, IBookingService bookingService, ICustomerContext customerContext)
        {
            _etgOptions = etgOptions.Value;
            _dataProviderFactory = dataProviderFactory;
            _bookingManager = bookingManager;
            _customerContext = customerContext;
            _bookingService = bookingService;
        }
        
        
        public async Task<Result> ProcessBookingStatus(EtgBookingResponse bookingResponse)
        {
            if (!CheckSignature(bookingResponse.Signature))
                return Result.Fail("Signature is invalid");

            var bookingReferenceCode = bookingResponse.Data.PartnerOrderId;
            var (_, isGetBookingFailure, booking, getBookingError) = await _bookingManager.Get(bookingReferenceCode);
            
            if (isGetBookingFailure)
                return Result.Fail(getBookingError);
            
            var dataProvider = _dataProviderFactory.Get(DataProviders.Etg);

            var (_, isGetBookingDetailsFailure, bookingDetails, getBookingDetailsError) =
                await dataProvider.GetBookingDetails(bookingResponse.Data.PartnerOrderId, booking.LanguageCode);
            if (isGetBookingDetailsFailure)
                return Result.Fail(getBookingDetailsError.Detail);
                
            await _customerContext.SetCustomerInfo(booking.CustomerId);
            
            return await _bookingService.ProcessResponse(bookingDetails, booking); 
        }


        private bool CheckSignature(EtgBookingResponseSignature responseSignature)
        {
            var currentTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (currentTimestamp - responseSignature.Timestamp > MaxTimestamp)
                return false;
            
            var hash = new HMACSHA256(Encoding.ASCII.GetBytes(_etgOptions.ApiKey));
            var timestampAndTokenBytes= Encoding.ASCII.GetBytes(responseSignature.Timestamp + responseSignature.Token);
            var hashBytes = hash.ComputeHash(timestampAndTokenBytes);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            
            return hashString.Equals(responseSignature.Signature);
        }


        private const long MaxTimestamp = 30;
        private readonly EtgOptions _etgOptions;
        private readonly IDataProviderFactory _dataProviderFactory;
        private readonly IBookingManager _bookingManager;
        private readonly ICustomerContext _customerContext;
        private readonly IBookingService _bookingService;
    }
}