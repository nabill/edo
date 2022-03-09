using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups.Templates
{
    public interface IMarkupPolicyTemplateService
    {
        string GetMarkupsFormula(IEnumerable<MarkupPolicy> policies);
    }
}