using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Locations;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public interface ILocationService
    {
        Task<List<Country>> GetCountries(string query, string languageCode);

        Task<List<Region>> GetRegions(string languageCode);
    }
}