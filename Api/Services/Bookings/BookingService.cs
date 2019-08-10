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
        public BookingService(IOptions<DataProviderOptions> options, INetClient netClient)
        {
            _netClient = netClient;
            _options = options.Value;
        }


        public Task<Result<HotelBookingDetails, ProblemDetails>> BookHotel(HotelBookingRequest request, string languageCode)
            => _netClient.Post<HotelBookingRequest, HotelBookingDetails>(new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute), 
                request, languageCode);


        private readonly INetClient _netClient;
        private readonly DataProviderOptions _options;
    }
}