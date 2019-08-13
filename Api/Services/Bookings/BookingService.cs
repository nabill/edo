using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Bookings
{
    public class BookingService : IBookingService
    {
        public BookingService(IOptions<DataProviderOptions> options, IDataProviderClient dataProviderClient)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
        }


        public Task<Result<HotelBookingDetails, ProblemDetails>> BookAccommodation(AccommodationBookingRequest request, string languageCode)
            => _dataProviderClient.Post<AccommodationBookingRequest, HotelBookingDetails>(new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute), 
                request, languageCode);


        private readonly IDataProviderClient _dataProviderClient;
        private readonly DataProviderOptions _options;
    }
}