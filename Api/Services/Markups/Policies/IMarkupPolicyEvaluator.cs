using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Markups.Policies
{
    public interface IMarkupPolicyEvaluator
    {
        decimal Evaluate(decimal price, Currencies currency);
    }
}