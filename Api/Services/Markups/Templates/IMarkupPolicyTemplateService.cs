using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Markups.Templates
{
    public interface IMarkupPolicyTemplateService
    {
        IReadOnlyCollection<MarkupPolicyTemplate> Get();
        Result<Expression<Func<decimal, decimal>>> CreateExpression(int templateId, IDictionary<string, decimal> settings);
    }
}