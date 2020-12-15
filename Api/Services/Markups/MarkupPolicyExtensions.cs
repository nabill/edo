using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public static class MarkupPolicyExtensions
    {
        public static MarkupPolicySettings GetSettings(this MarkupPolicy policy)
            => new (policy.Description, policy.TemplateId, policy.TemplateSettings, policy.Order, policy.Currency);
    }
}