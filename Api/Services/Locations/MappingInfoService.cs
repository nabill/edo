using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class MappingInfoService : IMappingInfoService
    {
        public MappingInfoService(IAccommodationMapperClient mapperClient)
        {
            _mapperClient = mapperClient;
        }
        
        
        /// <summary>
        /// Receives information about locality
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Result<SlimMappingLocalityInfo>> GetSlimMappingLocalityInfo(string id)
        {
            var (_, isFailure, value, _) = await _mapperClient.GetMappings(new List<string> { id }, "en");
            if (isFailure)
                return Result.Failure<SlimMappingLocalityInfo>("Cannot get info for locality");

            var location = value.FirstOrDefault().Location;
            return new SlimMappingLocalityInfo
            {
                Country = location.Country,
                CountryHtId = location.CountryHtId,
                Locality = location.Locality,
                LocalityHtId = location.LocalityHtId
            };
        }


        private readonly IAccommodationMapperClient _mapperClient;
    }
}