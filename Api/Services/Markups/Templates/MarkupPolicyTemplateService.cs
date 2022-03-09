using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Markups.Templates
{
    public class MarkupPolicyTemplateService : IMarkupPolicyTemplateService
    {
        public string GetMarkupsFormula(IEnumerable<MarkupPolicy> policies)
        {
            decimal multiplier = 1;
            var additions = new Dictionary<Currencies, decimal>();

            foreach (var policy in policies.OrderBy(p => p.FunctionType))
            {
                if (policy.FunctionType == MarkupFunctionType.Percent)
                {
                    multiplier *= (100 + policy.Value) / 100;
                }

                if (policy.FunctionType == MarkupFunctionType.Fixed)
                {
                    additions.TryGetValue(policy.Currency, out var currentValue);
                    additions[policy.Currency] = currentValue + policy.Value;
                }
            }

            var multPart = multiplier == 1m ? "x" : $"x * {multiplier.ToString(CultureInfo.InvariantCulture)}";
            var addPart = string.Join(" + ", additions.Select(x => $"{x.Value.ToString(CultureInfo.InvariantCulture)} {x.Key}"));

            var wholePart = multPart;
            if (!string.IsNullOrWhiteSpace(addPart))
                wholePart += $" + {addPart}";

            return wholePart;
        }
    }
}