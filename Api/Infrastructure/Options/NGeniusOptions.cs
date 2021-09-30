using System.Collections.Generic;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class NGeniusOptions
    {
        public string ApiKey { get; set; }
        public string Host { get; set; }
        public Dictionary<Currencies, string> Outlets { get; set; }
    }
}