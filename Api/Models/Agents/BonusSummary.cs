using System.Collections.Generic;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct BonusSummary
    {
        public List<MoneyAmount> Summary { get; init; }
    }
}