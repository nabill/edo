using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Markups.Policies
{
    public class MultiplyingMarkupPolicy : IMarkupPolicyEvaluator
    {
        private readonly MultiplyingMarkupSettings _policySettings;
        
        public MultiplyingMarkupPolicy(MultiplyingMarkupSettings policySettings)
        {
            _policySettings = policySettings;
        }
        
        public decimal Evaluate(decimal price, Currencies currency)
        {
            return price * _policySettings.Factor;
        }
    }
}