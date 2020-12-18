using System;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct Bonus
    {
        public string ReferenceCode { get; init; }
        public DateTime? Paid { get; init; }
        public MoneyAmount Amount { get; init; }
    }
}