using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupPolicyStorage
    {
        List<MarkupPolicy> Get(Func<MarkupPolicy, bool> predicate);

        void Set(List<MarkupPolicy> markupPolicies);
    }
}