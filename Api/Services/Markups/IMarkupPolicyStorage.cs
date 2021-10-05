using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupPolicyStorage
    {
        Task<List<MarkupPolicy>> Get(Func<MarkupPolicy, bool> predicate);
    }
}