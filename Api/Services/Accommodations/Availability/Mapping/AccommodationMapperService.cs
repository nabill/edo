using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class AccommodationMapperService : IAccommodationMapperService
    {
        public AccommodationMapperService(IAccommodationMapperClient client)
        {
            _client = client;
        }


        public async Task<Result<LocationDescriptor>> GetLocationDescriptor(string htId)
        {
            throw new NotImplementedException();
        }


        private readonly IAccommodationMapperClient _client;
    }
}