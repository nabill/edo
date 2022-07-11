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
        Task<Result> Add(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default);
        Task<List<Market>> Get(CancellationToken cancellationToken = default);
        Task<Result> Update(string languageCode, MarketRequest marketRequest, CancellationToken cancellationToken = default);
        Task<Result> Remove(int marketId, CancellationToken cancellationToken = default);
        Task<Result> UpdateMarketCountries(CountryRequest countryRequest, CancellationToken cancellationToken = default);
        Task<Result<List<CountrySlim>>> GetMarketCountries(int marketId, CancellationToken cancellationToken = default);
    }
}