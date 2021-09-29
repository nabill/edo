using System.Collections.Generic;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class NGeniusOptions
    {
        public string Token { get; set; }
        public string Endpoint { get; set; }
        public Dictionary<Currencies, string> Outlets { get; set; }
    }
}