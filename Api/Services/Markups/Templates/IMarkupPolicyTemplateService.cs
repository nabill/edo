using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups.Templates
{
    public interface IMarkupPolicyTemplateService
    {
        Func<decimal, decimal> CreateFunction(int templateId, IDictionary<string, decimal> settings);

        Result Validate(int templateId, IDictionary<string, decimal> settings);

        string GetMarkupsFormula(IEnumerable<MarkupPolicy> policies);
    }
}