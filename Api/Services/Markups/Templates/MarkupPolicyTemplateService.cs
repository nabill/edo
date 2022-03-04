using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Markups.Templates
{
    public class MarkupPolicyTemplateService : IMarkupPolicyTemplateService
    {
        public Func<decimal, decimal> CreateFunction(MarkupFunctionType functionType, decimal value)
        {
            var (_, isFailure, error) = Validate(functionType, value);
            // This is not normal case but it would be better to double check this to avoid errors.
            if (isFailure)
                throw new Exception(error);

            return functionType switch
            {
                MarkupFunctionType.Percent => v => v * (100 + value) / 100,
                MarkupFunctionType.Fixed => v => v + value,
                _ => throw new ArgumentOutOfRangeException(nameof(functionType), functionType, null)
            };
        }


        public Result Validate(MarkupFunctionType functionType, decimal value)
        {
            return functionType switch
            {
                MarkupFunctionType.Percent => ValidatePercent(value),
                MarkupFunctionType.Fixed => ValidateFixed(value),
                _ => Result.Failure("Invalid function type")
            };


            static Result ValidatePercent(decimal value)
            {
                return value > 0
                    ? Result.Success()
                    : Result.Failure("Percent can not be below zero");
            }


            static Result ValidateFixed(decimal value)
            {
                return value > 0
                    ? Result.Success()
                    : Result.Failure("Fixed markup can not be below zero");
            }
        }

        
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