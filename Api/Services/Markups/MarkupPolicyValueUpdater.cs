using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups;

/// <summary>
/// Temporary class to not break markup management logic before we finish migration to template-free markups
/// </summary>
public static class MarkupPolicyValueUpdater
{
    public static void FillValuesFromTemplateSettings(MarkupPolicy policy, IDictionary<string, decimal> templateSettings)
    {
        switch (policy.TemplateId)
        {
            case MarkupPolicyTemplateService.MultiplicationTemplateId:
                policy.FunctionType = MarkupFunctionType.Percent;
                policy.Value = (templateSettings["factor"] * 100) - 100;
                break;
            case MarkupPolicyTemplateService.AdditionTemplateId:
                policy.FunctionType = MarkupFunctionType.Fixed;
                policy.Value = templateSettings["addition"];
                break;
            default:
                throw new ArgumentException("Invalid template id", nameof(policy.TemplateId));
        }
    }
}