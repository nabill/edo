using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationService : IAccommodationService
    {
        public AccommodationService(EdoContext context, IMemoryFlow flow, IOptions<DataProviderOptions> options, IDataProviderClient dataProviderClient, ILocationService locationService)
        {
            _context = context;
            _flow = flow;
            _dataProviderClient = dataProviderClient;
            _locationService = locationService;

            _options = options.Value;
        }


        public ValueTask<Result<RichAccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(AccommodationService), "Accommodations", languageCode, accommodationId),
                async () => await _dataProviderClient.Get<RichAccommodationDetails>(new Uri($"{_options.Netstorming}hotels/{accommodationId}", UriKind.Absolute),
                    languageCode),
                TimeSpan.FromDays(1));


        public async ValueTask<Result<AvailabilityResponse, ProblemDetails>> GetAvailable(AvailabilityRequest request, string languageCode)
        {
            var (_, isFailure, location, error) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Fail<AvailabilityResponse, ProblemDetails>(error);

            return await _dataProviderClient.Post<InnerAvailabilityRequest, AvailabilityResponse>(new Uri(_options.Netstorming + "hotels/availability", UriKind.Absolute),
                new InnerAvailabilityRequest(request, location), languageCode);
        }


        public async Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest request, string languageCode)
        {
            var itn = await _context.GetNextItineraryNumber();
            var referenceCode = ReferenceCodeGenerator.Generate(ServiceTypes.HTL, request.Residency, itn);

            var inner = new InnerAccommodationBookingRequest(request, referenceCode);

            return await _dataProviderClient.Post<InnerAccommodationBookingRequest, AccommodationBookingDetails>(
                new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute),
                inner, languageCode);
        }


        private readonly EdoContext _context;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly IMemoryFlow _flow;
        private readonly ILocationService _locationService;
        private readonly DataProviderOptions _options;
    }
}