using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public class MapperService : IMapperService
    {
        public MapperService(IMapperClient client)
        {
            _client = client;
        }


        public async Task<Result<LocationDescriptor>> GetLocationDescriptor(string htId)
        {
            throw new NotImplementedException();
        }


        private readonly IMapperClient _client;
    }
}