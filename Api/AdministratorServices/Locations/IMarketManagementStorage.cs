using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.Locations;

namespace Api.AdministratorServices.Locations
{
    public interface IMarketManagementStorage
    {
        Task<List<Market>> Get(CancellationToken cancellationToken);
        Task<List<Country>> GetCountries(int marketId, CancellationToken cancellationToken);
        Task<Market?> Get(int marketId, CancellationToken cancellationToken);
        Task Refresh(CancellationToken cancellationToken);
        Task RefreshCountries(int marketId, CancellationToken cancellationToken);
    }
}