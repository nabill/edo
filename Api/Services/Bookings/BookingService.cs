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



        public async Task<Result<AccommodationBookingDetails, ProblemDetails>> BookAccommodation(AccommodationBookingRequest request, string languageCode)
        {
            long idn = await _context.GetNextIdentityValue();

            string referenceCode = ReferenceCodeGenerator.Generate(ServiceType.HotelBooking, request.Residency, idn);

            var inner = new InnerAccommodationBookingRequest(request, referenceCode);

            return await _dataProviderClient.Post<InnerAccommodationBookingRequest, AccommodationBookingDetails>(
                new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute),
                inner, languageCode);
        }



        private readonly EdoContext _context;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly DataProviderOptions _options;
    }
}