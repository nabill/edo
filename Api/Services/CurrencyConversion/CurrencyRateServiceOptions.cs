using System;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyRateServiceOptions
    {
        public Uri ServiceUrl { get; set; }
        public TimeSpan CacheLifeTime { get; set; }
    }
}