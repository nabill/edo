using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyRateService : ICurrencyRateService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryFlow _memoryFlow;


        public CurrencyRateService(IHttpClientFactory httpClientFactory,
            IOptions<CurrencyRateServiceOptions> options,
            IMemoryFlow memoryFlow)
        {
            _httpClientFactory = httpClientFactory;
            _memoryFlow = memoryFlow;
            _options = options.Value;
        }

        public ValueTask<decimal> Get(Currencies source, Currencies target)
        {
            if (source == target)
                return SameCurrencyRateResult;

            var key = _memoryFlow.BuildKey(nameof(CurrencyRateService), source.ToString(), target.ToString());
            return _memoryFlow.GetOrSetAsync(key, () => GetCurrent(source, target), 
                _options.CacheLifeTime);
        }


        private async Task<decimal> GetCurrent(Currencies source, Currencies target)
        {
            var rate = await _httpClientFactory.CreateClient(HttpClientNames.CurrencyService)
                .GetStringAsync(_options.ServiceUrl + $"/api/1.0/rates/{source}/{target}");

            return decimal.Parse(rate, CultureInfo.InvariantCulture);
        }


        private static readonly ValueTask<decimal> SameCurrencyRateResult = new ValueTask<decimal>(1);
        private readonly CurrencyRateServiceOptions _options;
    }
}