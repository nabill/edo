using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Locations;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;

namespace Api.AdministratorServices.Locations
{
    public interface IMarketManagementService
    {
        Task<Result> AddMarket(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default);
        Task<List<Market>> GetMarkets(string languageCode, CancellationToken cancellationToken = default);
        Task<Result> ModifyMarket(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default);
        Task<Result> RemoveMarket(MarketRequest marketRequest, CancellationToken cancellationToken = default);
    }
}