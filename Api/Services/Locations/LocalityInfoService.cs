using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public class LocalityInfoService : ILocalityInfoService
    {
        public LocalityInfoService(IAccommodationMapperClient mapperClient)
        {
            _mapperClient = mapperClient;
        }
        
        
        /// <summary>
        /// Receives information about locality
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Result<LocalityInfo>> GetLocalityInfo(string id)
        {
            var (_, isFailure, localityInfo, _) = await _mapperClient.GetLocalityInfo(id);
            if (isFailure)
                return Result.Failure<LocalityInfo>("Cannot get info for locality");
            
            return new LocalityInfo
            {
                CountryIsoCode = localityInfo.CountryIsoCode,
                CountryName = localityInfo.CountryName,
                CountryHtId = localityInfo.CountryHtId,
                LocalityName = localityInfo.LocalityName,
                LocalityHtId = localityInfo.LocalityHtId
            };
        }


        private readonly IAccommodationMapperClient _mapperClient;
    }
}