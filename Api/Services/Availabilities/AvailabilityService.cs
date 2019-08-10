using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Availabilities
{
    public class AvailabilityService : IAvailabilityService
    {
        public AvailabilityService(ILocationService locationService, INetClient netClient, IOptions<DataProviderOptions> options)
        {
            _locationService = locationService;
            _netClient = netClient;
            _options = options.Value;
        }


        public async ValueTask<Result<AvailabilityResponse, ProblemDetails>> Get(AvailabilityRequest request, string languageCode)
        {
            var (_, isFailure, location, error) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Fail<AvailabilityResponse, ProblemDetails>(error);

            return await _netClient.Post<InnerAvailabilityRequest, AvailabilityResponse>(new Uri(_options.Netstorming + "hotels/availability", UriKind.Absolute),
                new InnerAvailabilityRequest(request, location), languageCode);
        }


        private readonly ILocationService _locationService;
        private readonly INetClient _netClient;
        private readonly DataProviderOptions _options;
    }
}