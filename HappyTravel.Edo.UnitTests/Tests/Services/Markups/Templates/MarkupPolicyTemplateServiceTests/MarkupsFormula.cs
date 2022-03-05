using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Enums;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.Templates.MarkupPolicyTemplateServiceTests
{
    public class MarkupsFormula
    {
        [Theory]
        [MemberData(nameof(MarkupArrangements))]
        public void Formula_should_match_expected(List<MarkupPolicy> policies, string expectedFormula)
        {
            var formula = TemplateService.GetMarkupsFormula(policies);
            Assert.Equal(expectedFormula, formula);
        }

        public static readonly IEnumerable<object[]> MarkupArrangements = new[]
        {
            new object[]{
                new List<MarkupPolicy>
                {
                    new () {FunctionType = MarkupFunctionType.Percent, Value = 150}
                },
                "x * 2.5"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 2.5m, Currency = Currencies.USD}
                },
                "x + 2.5 USD"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 1m, Currency = Currencies.USD},
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 2m, Currency = Currencies.EUR}
                },
                "x + 1 USD + 2 EUR"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 1m, Currency = Currencies.USD},
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 2m, Currency = Currencies.USD}
                },
                "x + 3 USD"},
            new object[]{
                new List<MarkupPolicy>(),
                "x"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 2.5m, Currency = Currencies.USD},
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 2m, Currency = Currencies.EUR},
                    new () {FunctionType = MarkupFunctionType.Percent, Value = 150m},
                },
                "x * 2.5 + 2.5 USD + 2 EUR"},
            new object[]{
                new List<MarkupPolicy>
                {
                    new () {FunctionType = MarkupFunctionType.Fixed, Value = 1.123456789m, Currency = Currencies.USD},
                },
                "x + 1.123456789 USD"}
        };

        private static readonly MarkupPolicyTemplateService TemplateService = new MarkupPolicyTemplateService();
    }
}