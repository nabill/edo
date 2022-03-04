using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Data.Markup;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.Templates.MarkupPolicyTemplateServiceTests
{
    public class AdditionTemplateCalculations
    {
        [Theory]
        [MemberData(nameof(InvalidSettings))]
        public void Invalid_settings_should_fail(decimal value)
        {
            var validateResult = TemplateService.Validate(MarkupFunctionType.Fixed, value);
            
            Assert.True(validateResult.IsFailure);
        }
        
        [Theory]
        [MemberData(nameof(ValidSettings))]
        public void Valid_settings_should_not_fail(decimal value)
        {
            var validateResult = TemplateService.Validate(MarkupFunctionType.Fixed, value);
            
            Assert.True(validateResult.IsSuccess);
        }

        [Theory]
        [MemberData(nameof(SettingsAndResults))]
        public void Template_function_should_add(decimal supplierPrice, decimal value, decimal expectedResultPrice)
        {
            var function = TemplateService.CreateFunction(MarkupFunctionType.Fixed, value);
            
            var resultPrice = function(supplierPrice);
            
            Assert.Equal(expectedResultPrice, resultPrice);
        }

        public static readonly IEnumerable<object?[]> InvalidSettings = new[]
        {
            new object?[] {-1},
            new object?[] {0},
        };

        public static readonly IEnumerable<object[]> ValidSettings = new[]
        {
            new object[] {1},
            new object[] {333.33},
        };
        
        public static readonly IEnumerable<object[]> SettingsAndResults = new[]
        {
            new object[] {100, 10, 110},
            new object[] {20, 1.1, 21.1},
            new object[] {3, 99, 102},
            new object[] {45.5, 2.18, 47.68}
        };

        private static readonly MarkupPolicyTemplateService TemplateService = new MarkupPolicyTemplateService();
    }
}