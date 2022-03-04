using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Func<decimal, decimal> CreateFunction(int templateId, IDictionary<string, decimal> settings)
        {
            var template = Templates.Single(t => t.Id == templateId);
            var (_, isFailure, error) = Validate(template, settings);
            // This is not normal case but it would be better to double check this to avoid errors.
            if (isFailure)
                throw new Exception(error);

            return template.FunctionFactory(settings);
        }


        public Result Validate(int templateId, IDictionary<string, decimal> settings)
        {
            if (settings is null)
                return Result.Failure<MarkupPolicyTemplate>("Invalid settings");

            var template = Templates.SingleOrDefault(t => t.Id == templateId);
            if (template == default)
                return Result.Failure<MarkupPolicyTemplate>($"Could not find template by id {templateId}");

            return Validate(template, settings);
        }


        private Result Validate(MarkupPolicyTemplate template, IDictionary<string, decimal> settings)
        {
            if (!template.IsEnabled)
                return Result.Failure<MarkupPolicyTemplate>("Could not create expression for disabled template");

            if (!template.SettingsValidator(settings))
                return Result.Failure<MarkupPolicyTemplate>("Invalid template settings");

            return Result.Success();
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