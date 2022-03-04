using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups.Templates
{
    public interface IMarkupPolicyTemplateService
    {
        Func<decimal, decimal> CreateFunction(MarkupFunctionType functionType, decimal value);

        Result Validate(MarkupFunctionType functionType, decimal value);

        string GetMarkupsFormula(IEnumerable<MarkupPolicy> policies);
    }
}