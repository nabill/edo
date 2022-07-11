using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.Locations;

namespace Api.AdministratorServices.Locations
{
    public interface ICountryManagementStorage
    {
        Task<List<Country>> Get(CancellationToken cancellationToken);
        Task Refresh(CancellationToken cancellationToken);
    }
}