using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Enums;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.Templates.MarkupPolicyTemplateService
{
    public class MarkupsFormula
    {
        [Theory]
        [MemberData(nameof(MarkupArrangements))]
        public void Formula_should_match_expected(List<MarkupPolicy> policies, string expectedFormula)
        {
            var formula = TemplateService.GetMarkupsFormula(policies);
            Assert.Equal(formula, expectedFormula);
        }

        public static readonly IEnumerable<object[]> MarkupArrangements = new[]
        {
            new object[]{
                new List<MarkupPolicy>
                {
                    new MarkupPolicy {TemplateId = 1, TemplateSettings = new Dictionary<string, decimal>{["factor"] = 2.5m}}
                },
                "x * 2.5"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 2.5m}, Currency = Currencies.USD}
                },
                "x + 2.5 USD"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 1m}, Currency = Currencies.USD},
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 2m}, Currency = Currencies.EUR}
                },
                "x + 1 USD + 2 EUR"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 1m}, Currency = Currencies.USD},
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 2m}, Currency = Currencies.USD}
                },
                "x + 3 USD"},
            new object[]{
                new List<MarkupPolicy>(),
                "x"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 1m}, Currency = Currencies.USD},
                    new MarkupPolicy {TemplateId = 1, TemplateSettings = new Dictionary<string, decimal>{["factor"] = 2.5m}},
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 2m}, Currency = Currencies.EUR}
                },
                "x * 2.5 + 2.5 USD + 2 EUR"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new MarkupPolicy {TemplateId = 2, TemplateSettings = new Dictionary<string, decimal>{["addition"] = 1.123456789m}, Currency = Currencies.USD},
                },
                "x + 1.123456789 USD"},
        };

        private static readonly Api.Services.Markups.Templates.MarkupPolicyTemplateService TemplateService = new Api.Services.Markups.Templates.MarkupPolicyTemplateService();
    }
}