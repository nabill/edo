using System;
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
using Tsutsujigasaki.GrpcContracts.Models;
using Tsutsujigasaki.GrpcContracts.Services;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyRateService : ICurrencyRateService
    {
        public CurrencyRateService(IHttpClientFactory httpClientFactory,
            IOptionsMonitor<CurrencyRateServiceOptions> options,
            IDoubleFlow flow,
            ILogger<CurrencyRateService> logger,
            IRatesGrpcService ratesGrpcService)
        {
            _httpClientFactory = httpClientFactory;
            _flow = flow;
            _options = options;
            _logger = logger;
            _ratesGrpcService = ratesGrpcService;
        }

        public async ValueTask<Result<decimal>> Get(Currencies source, Currencies target)
        {
            if (source == target)
                return SameCurrencyRateResult;

            var key = _flow.BuildKey(nameof(CurrencyRateService), source.ToString(), target.ToString());
            return await _flow.GetOrSetAsync(key, () => GetCurrent(source, target), 
                _options.CurrentValue.CacheLifeTime);
        }


        private Task<Result<decimal>> GetCurrent(Currencies source, Currencies target)
        {
            return _options.CurrentValue.ClientType switch
            {
                ClientTypes.Grpc => GetFromGrpc(source, target),
                ClientTypes.WebApi => GetFromWebApi(source, target),
                _ => throw new NotSupportedException($"ClientType `{_options.CurrentValue.ClientType}` not supported")
            };
        }


        private async Task<Result<decimal>> GetFromGrpc(Currencies source, Currencies target)
        {
            var response = await _ratesGrpcService.GetRate(new RatesRequest
            {
                SourceCurrency = source,
                TargetCurrency = target
            });

            if (!response.Rate.IsFailure)
                return Result.Success(response.Rate.Value);

            _logger.LogCurrencyConversionFailed(source, target, response.Rate.Error);
            return Result.Failure<decimal>(response.Rate.Error);
        }


        private async Task<Result<decimal>> GetFromWebApi(Currencies source, Currencies target)
        {
            using var response = await _httpClientFactory
                .CreateClient(HttpClientNames.CurrencyService)
                .GetAsync($"api/1.0/rates/{source}/{target}");

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

        
        private static readonly Result<decimal> SameCurrencyRateResult =  Result.Success(1m);
        

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDoubleFlow _flow;
        private readonly ILogger<CurrencyRateService> _logger;
        private readonly IOptionsMonitor<CurrencyRateServiceOptions> _options;
        private readonly IRatesGrpcService _ratesGrpcService;
    }
}