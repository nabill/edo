using HappyTravel.Edo.Common.Enums.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupPolicyManagerFactory
    {
        IMarkupPolicyManager Get(MarkupPolicyManagerTypes type);
    }
}