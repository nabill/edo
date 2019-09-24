using HappyTravel.Edo.Common.Enums.Markup;

namespace HappyTravel.Edo.Data.Markup
{
    public class MarkupPolicy
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int Order { get; set; }
        public MarkupPolicyScope Scope { get; set; }
        public MarkupPolicyType Type { get; set; }
        public MarkupPolicyTarget Target { get; set; }
        public string Settings { get; set; }
    }
}