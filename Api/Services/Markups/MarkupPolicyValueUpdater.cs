using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups;

/// <summary>
/// Temporary class to not break markup management logic before we finish migration to template-free markups
/// </summary>
public static class MarkupPolicyValueUpdater
{
    public static void FillValuesFromTemplateSettings(MarkupPolicy policy, MarkupFunctionType functionType, decimal value)
    {
        policy.FunctionType = functionType;
        policy.Value = value;
    }
}