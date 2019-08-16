using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HappyTravel.Edo.Api.Infrastructure.Constants;

namespace HappyTravel.Edo.Api.Services.Bookings
{
    public class BookingService : IBookingService
    {
        public BookingService(IOptions<DataProviderOptions> options, IDataProviderClient dataProviderClient, EdoContext context)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
            _context = context;
        }


        public async Task<Result<HotelBookingDetails, ProblemDetails>> BookHotel(HotelBookingRequest request,
            string languageCode)
        {
            long idn = await _context.GetNextIdentityValue();

            request.ReferenceCode = ReferenceCodeGenerator.Generate(ServiceType.HotelBooking, request.CityCode, idn);

            return await _dataProviderClient.Post<HotelBookingRequest, HotelBookingDetails>(
                 new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute),
                 request, languageCode);
        }



        private readonly EdoContext _context;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly DataProviderOptions _options;
    }
}