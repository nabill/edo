using System;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public class CurrencyRateServiceOptions
    {
        public TimeSpan CacheLifeTime { get; set; }
        public ClientTypes ClientType { get; set; }
    }
}