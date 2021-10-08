using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Data.Markup;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyStorage : IMarkupPolicyStorage
    {
        public MarkupPolicyStorage(IOptionsMonitor<MarkupPolicyStorageOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }
        
        
        public List<MarkupPolicy> Get(Func<MarkupPolicy, bool> predicate)
        {
            if (SpinWait.SpinUntil(() => _isFilled, _optionsMonitor.CurrentValue.Timeout))
                return _storage.Where(predicate).ToList();

            throw new Exception("Markup policy storage is not filled");
        }


        public void Set(List<MarkupPolicy> markupPolicies)
        {
            _storage = markupPolicies;
            _isFilled = true;
        }


        private volatile List<MarkupPolicy> _storage = new();
        private bool _isFilled;


        private readonly IOptionsMonitor<MarkupPolicyStorageOptions> _optionsMonitor;
    }
}