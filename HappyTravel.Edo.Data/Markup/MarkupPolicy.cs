using System;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Markup
{
    public class MarkupPolicy
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public Currencies Currency { get; set; }
        public SubjectMarkupScopeTypes SubjectScopeType { get; set; }
        public string SubjectScopeId { get; set; }
        public DestinationMarkupScopeTypes DestinationScopeType { get; set; }
        public string DestinationScopeId { get; set; }
        public MarkupFunctionType FunctionType { get; set; }
        public decimal Value { get; set; }
    }
}