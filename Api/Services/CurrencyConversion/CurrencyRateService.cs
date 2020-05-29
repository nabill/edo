using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyRateService : ICurrencyRateService
    {
        public CurrencyRateService(IHttpClientFactory httpClientFactory,
            IOptions<CurrencyRateServiceOptions> options,
            IMemoryFlow memoryFlow)
        {
            _httpClientFactory = httpClientFactory;
            _memoryFlow = memoryFlow;
            _options = options.Value;
        }

        public ValueTask<Result<decimal>> Get(Currencies source, Currencies target)
        {
            if (source == target)
                return SameCurrencyRateResult;

            var key = _memoryFlow.BuildKey(nameof(CurrencyRateService), source.ToString(), target.ToString());
            return _memoryFlow.GetOrSetAsync(key, () => GetCurrent(source, target), 
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
                // TODO: Add logging
                return Result.Failure<decimal>(error.Detail);
            }
            
            if (!response.IsSuccessStatusCode)
            {
                // TODO: Add logging
                return Result.Failure<decimal>("Currency conversion error");
            }
            
            return Result.Ok(decimal.Parse(content, CultureInfo.InvariantCulture));
        }
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryFlow _memoryFlow;

        private static readonly ValueTask<Result<decimal>> SameCurrencyRateResult = new ValueTask<Result<decimal>>(Result.Ok((decimal)1));
        private readonly CurrencyRateServiceOptions _options;
    }
}