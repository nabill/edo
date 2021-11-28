using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Data.Markup;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class DiscountStorage : IDiscountStorage
    {
        public DiscountStorage(IOptionsMonitor<DiscountStorageOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }
        
        
        public List<Discount> Get(Func<Discount, bool> predicate)
        {
            if (SpinWait.SpinUntil(() => _isFilled, _optionsMonitor.CurrentValue.Timeout))
                return _storage.Where(predicate).ToList();

            throw new Exception("Discount storage is not filled");
        }


        public void Set(List<Discount> discounts)
        {
            _storage = discounts;
            _isFilled = true;
        }
        
        
        private volatile List<Discount> _storage = new();
        private bool _isFilled;


        private readonly IOptionsMonitor<DiscountStorageOptions> _optionsMonitor;
    }
}