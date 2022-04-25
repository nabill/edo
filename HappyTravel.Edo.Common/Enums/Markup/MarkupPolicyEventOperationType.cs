using System.ComponentModel;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    public enum MarkupPolicyEventOperationType
    {
        [Description("None")]
        None = 0,
        [Description("Created")]
        Created = 1,
        [Description("Modified")]
        Modified = 2,
        [Description("Deleted")]
        Deleted = 3
    }
}