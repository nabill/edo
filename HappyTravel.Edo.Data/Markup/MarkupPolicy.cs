using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using HappyTravel.Edo.Common.Enums.Markup;

namespace HappyTravel.Edo.Data.Markup
{
    public class MarkupPolicy
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int? CustomerId { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        public int Order { get; set; }
        public MarkupPolicyScopeType ScopeType { get; set; }
        public MarkupPolicyTarget Target { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public Expression<Func<decimal, decimal>> Function { get; set; }
        public int TemplateId { get; set; }
        public Dictionary<string, decimal> TemplateSettings { get; set; }
    }
}