using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Locations;

namespace Api.AdministratorServices.Locations
{
    public interface ICountryManagementService
    {
        Task<List<Country>> Get(CancellationToken cancellationToken = default);
    }
}