using System;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AccommodationMapperClient : IAccommodationMapperClient
    {
        public AccommodationMapperClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        
        public Task<Result<LocationMapping>> GetMapping(string htId)
        {
            throw new NotImplementedException();
        }
        
        private readonly IHttpClientFactory _clientFactory;
    }
}