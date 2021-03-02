using System;
using HappyTravel.Edo.Common.Enums.Markup;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MaterializationData
    {
        public int PolicyId { get; init; }
        public string ReferenceCode { get; init; }
        public int AgencyId { get; init; }
        public decimal Amount { get; init; }
        public MarkupPolicyScopeType ScopeType { get; init; }
    }
}