using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyRateService : ICurrencyRateService
    {
        public CurrencyRateService(IHttpClientFactory httpClientFactory,
            IOptions<CurrencyRateServiceOptions> options,
            IDoubleFlow flow,
            ILogger<CurrencyRateService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _flow = flow;
            _options = options.Value;
            _logger = logger;
        }

        public async ValueTask<Result<decimal>> Get(Currencies source, Currencies target)
        {
            if (source == target)
                return SameCurrencyRateResult;

            var key = _flow.BuildKey(nameof(CurrencyRateService), source.ToString(), target.ToString());
            return await _flow.GetOrSetAsync(key, () => GetCurrent(source, target), 
                _options.CacheLifeTime);
        }


        private async Task<Result<decimal>> GetCurrent(Currencies source, Currencies target)
        {
            using var response = await _httpClientFactory
                .CreateClient(HttpClientNames.CurrencyService)
                .GetAsync($"{_options.ServiceUrl}api/1.0/rates/{source}/{target}");

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = JsonConvert.DeserializeObject<ProblemDetails>(content);
                _logger.LogCurrencyConversionFailed(source, target, error.Detail);
                return Result.Failure<decimal>(error.Detail);
            }
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogCurrencyConversionFailed(source, target, content);
                return Result.Failure<decimal>("Currency conversion error");
            }
            
            return Result.Success(decimal.Parse(content, CultureInfo.InvariantCulture));
        }
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDoubleFlow _flow;

        private static readonly Result<decimal> SameCurrencyRateResult =  Result.Success((decimal)1);
        private readonly CurrencyRateServiceOptions _options;
        private ILogger<CurrencyRateService> _logger;
    }
}