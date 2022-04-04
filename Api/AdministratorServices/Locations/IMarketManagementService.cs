using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Locations;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Locations;

namespace Api.AdministratorServices.Locations
{
    public interface IMarketManagementService
    {
        Task<Result> Add(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default);
        Task<List<Market>> Get(CancellationToken cancellationToken = default);
        Task<Result> Update(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default);
        Task<Result> Remove(int marketId, CancellationToken cancellationToken = default);
    }
}