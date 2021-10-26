using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Markup
{
    public class MarkupPolicy
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public MarkupPolicyTarget Target { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int TemplateId { get; set; }
        public IDictionary<string, decimal> TemplateSettings { get; set; }
        public Currencies Currency { get; set; }
        public AgentMarkupScopeTypes AgentScopeType { get; set; }
        public string AgentScopeId { get; set; }
        public DestinationMarkupScopeTypes DestinationScopeType { get; set; }
        public string DestinationScopeId { get; set; }
    }
}