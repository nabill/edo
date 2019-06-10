using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Locations;

namespace HappyTravel.Edo.Api.Services.Data
{
    public interface ILocationService
    {
        ValueTask<List<Country>> GetCountries(string query, string languageCode);

        ValueTask<List<Region>> GetRegions(string languageCode);
    }
}