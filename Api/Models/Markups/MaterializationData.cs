using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MaterializationData
    {
        public int PolicyId { get; init; }
        public string ReferenceCode { get; init; }
        public string SubjectScopeId { get; init; }
        public MoneyAmount Amount { get; init; }
        public SubjectMarkupScopeTypes ScopeType { get; init; }
    }
}