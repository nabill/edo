using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class DataProvider : IDataProvider
    {
        public DataProvider(IDataProviderClient dataProviderClient, ILocationService locationService, string baseUrl)
        {
            _dataProviderClient = dataProviderClient;
            _locationService = locationService;
            _baseUrl = baseUrl;
        }
        
        
        public async Task<Result<AvailabilityDetails, ProblemDetails>> GetAvailability(AvailabilityRequest request, string languageCode)
        {
            return await _dataProviderClient.Post<AvailabilityRequest, AvailabilityDetails>(
                new Uri(_baseUrl + "availabilities/accommodations", UriKind.Absolute), request, languageCode);
        }


        public Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> GetAvailability(long availabilityId,
            string accommodationId, string languageCode)
        {
            return _dataProviderClient.Post<SingleAccommodationAvailabilityDetails>(
                new Uri(_baseUrl + "accommodations/" + accommodationId + "/availabilities/" + availabilityId, UriKind.Absolute), languageCode);
        }


        public Task<Result<DeadlineDetails, ProblemDetails>> GetDeadline(string accommodationId, string availabilityId, string agreementCode, string languageCode)
        {
            var uri = new Uri($"{_baseUrl}accommodations/{accommodationId}/deadline/{availabilityId}/{agreementCode}", UriKind.Absolute);
            return _dataProviderClient.Get<DeadlineDetails>(uri, languageCode);
        }


        public Task<Result<AccommodationDetails, ProblemDetails>> GetAccommodation(string accommodationId, string languageCode)
        {
            return _dataProviderClient.Get<AccommodationDetails>(
                new Uri($"{_baseUrl}accommodations/{accommodationId}", UriKind.Absolute), languageCode);
        }


        public Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> GetExactAvailability(long availabilityId, Guid agreementId, string languageCode)
        {
            return _dataProviderClient.Post<SingleAccommodationAvailabilityDetailsWithDeadline>(
                new Uri($"{_baseUrl}accommodations/availabilities/{availabilityId}/agreements/{agreementId}", UriKind.Absolute), languageCode);
        }
        
        private readonly IDataProviderClient _dataProviderClient;
        private readonly ILocationService _locationService;
        private readonly string _baseUrl;
    }
}