using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public static class MarkupPolicyExtensions
    {
        public static MarkupPolicySettings GetSettings(this MarkupPolicy policy)
        {
            // TODO Cleanup the model: https://github.com/happy-travel/agent-app-project/issues/777
            var locationScopeId = policy.SubjectScopeType == SubjectMarkupScopeTypes.Market || policy.SubjectScopeType == SubjectMarkupScopeTypes.Country || policy.SubjectScopeType == SubjectMarkupScopeTypes.Locality
                ? policy.SubjectScopeId
                : null;

            return new(description: policy.Description,
                functionType: policy.FunctionType,
                value: policy.Value,
                currency: policy.Currency,
                locationScopeId: locationScopeId!,
                destinationScopeId: policy.DestinationScopeId);
        }
    }
}