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
            var locationScopeId = policy.AgentScopeType == AgentMarkupScopeTypes.Country || policy.AgentScopeType == AgentMarkupScopeTypes.Locality
                ? policy.AgentScopeId
                : null;
            
            return new(description: policy.Description, 
                templateId: policy.TemplateId,
                templateSettings: policy.TemplateSettings,
                order: policy.Order,
                currency: policy.Currency, 
                locationScopeId: locationScopeId,
                destinationScopeId: policy.DestinationScopeId);
        }
    }
}