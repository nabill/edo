using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.MultiLanguage;

namespace Api.AdministratorServices.Locations
{
    public interface IMarketManagementService
    {
        Task<Result> AddMarket(string languageCode, MultiLanguage<string> namesRequest, CancellationToken cancellationToken = default);
        Task<List<Market>> GetMarkets(string languageCode, CancellationToken cancellationToken = default);
        Task<Result> ModifyMarket(string languageCode, int marketId, MultiLanguage<string> namesRequest, CancellationToken cancellationToken = default);
        Task<Result> RemoveMarket(int marketId, CancellationToken cancellationToken = default);
    }
}