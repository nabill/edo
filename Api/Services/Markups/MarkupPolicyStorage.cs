using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyStorage : IMarkupPolicyStorage
    {
        public List<MarkupPolicy> Get(Func<MarkupPolicy, bool> predicate) 
            => _storage.Where(predicate).ToList();


        public void Set(List<MarkupPolicy> markupPolicies) 
            => _storage = markupPolicies;


        private volatile List<MarkupPolicy> _storage = new();
    }
}