using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Locations;

namespace Api.AdministratorServices.Locations
{
    public interface IMarkupLocationService
    {
        Task<List<Region>> GetRegions(string languageCode);
    }
}