using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IDiscountStorage
    {
        List<Discount> Get(Func<Discount, bool> predicate);

        void Set(List<Discount> markupPolicies);
    }
}