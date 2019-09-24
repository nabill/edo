using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Markups.Policies
{
    public class AdditionMarkupPolicy : IMarkupPolicyEvaluator
    {
        public AdditionMarkupPolicy(AdditionMarkupPolicySettings policySettings)
        {
            _policySettings = policySettings;
        }

        public decimal Evaluate(decimal price, Currencies currency)
        {
            // TODO Currency conversion?
            if (_policySettings.Currency != currency)
                throw new ArgumentException("Invalid currency");

            return price + _policySettings.Addition;
        }
        
        private readonly AdditionMarkupPolicySettings _policySettings;
    }
}