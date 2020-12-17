using System;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct Bonus
    {
        public string ReferenceCode { get; init; }
        public DateTime? Paid { get; init; }
        public decimal Amount { get; init; }
    }
}