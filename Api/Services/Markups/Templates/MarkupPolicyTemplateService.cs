using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups.Templates;
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

        
        private const string MultiplyingFactorSetting = "factor";
        private const string AdditionValueSetting = "addition";

        // !! These templates are referenced from MarkupPolicies and should not be changed without appropriate migration.
        private static readonly MarkupPolicyTemplate[] Templates =
        {
            new MarkupPolicyTemplate
            {
                Id = MultiplicationTemplateId,
                Title = "Multiplication",
                ParameterNames = new[] {MultiplyingFactorSetting},
                IsEnabled = true,
                FunctionFactory = settings => rawValue => rawValue * settings[MultiplyingFactorSetting],
                SettingsValidator = settings => settings.Keys.Count == 1 &&
                    settings.ContainsKey(MultiplyingFactorSetting) &&
                    settings[MultiplyingFactorSetting] > 1
            },
            new MarkupPolicyTemplate
            {
                Id = AdditionTemplateId,
                Title = "Addition",
                ParameterNames = new[] {AdditionValueSetting},
                IsEnabled = true,
                FunctionFactory = settings => rawValue => rawValue + settings[AdditionValueSetting],
                SettingsValidator = settings => settings.Keys.Count == 1 &&
                    settings.ContainsKey(AdditionValueSetting) &&
                    settings[AdditionValueSetting] > 0
            }
        };


        public string GetMarkupsFormula(IEnumerable<MarkupPolicy> policies)
        {
            decimal multiplier = 1;
            var additions = new Dictionary<Currencies, decimal>();

            foreach (var policy in policies)
            {
                if (policy.TemplateId == MultiplicationTemplateId)
                {
                    multiplier *= policy.TemplateSettings[MultiplyingFactorSetting];
                    foreach (var key in additions.Keys.ToList())
                        additions[key] *= policy.TemplateSettings[MultiplyingFactorSetting];
                }

                if (policy.TemplateId == AdditionTemplateId)
                {
                    additions.TryGetValue(policy.Currency, out var currentValue);
                    additions[policy.Currency] = currentValue + policy.TemplateSettings[AdditionValueSetting];
                }
            }

            var multPart = multiplier == 1m ? "x" : $"x * {multiplier.ToString(CultureInfo.InvariantCulture)}";
            var addPart = string.Join(" + ", additions.Select(x => $"{x.Value.ToString(CultureInfo.InvariantCulture)} {x.Key}"));

            var wholePart = multPart;
            if (!string.IsNullOrWhiteSpace(addPart))
                wholePart += $" + {addPart}";

            return wholePart;
        }

        public const int MultiplicationTemplateId = 1;
        public const int AdditionTemplateId = 2;
    }
}