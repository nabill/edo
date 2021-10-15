using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class DiscountFunctionService : IDiscountFunctionService
    {
        public DiscountFunctionService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }
        
        
        public async ValueTask<PriceProcessFunction> Get(MarkupPolicy policy, MarkupSubjectInfo subject)
        {
            // Discounts are only supported for global markups for now
            if (policy.AgentScopeType != AgentMarkupScopeTypes.Global)
                return (price => new ValueTask<MoneyAmount>(price));

            var discountsKey = GetKey(policy, subject);
            var applicableDiscounts = await _flow.GetOrSetAsync(discountsKey, 
                GetAgentDiscounts,
                DiscountCacheLifeTime);

            return moneyAmount =>
            {
                var currentAmount = moneyAmount;
                foreach (var discount in applicableDiscounts)
                {
                    currentAmount = new MoneyAmount
                    {
                        Amount = currentAmount.Amount * (100 - discount.DiscountPercent) / 100,
                        Currency = currentAmount.Currency
                    };
                }

                return new ValueTask<MoneyAmount>(currentAmount);
            };
            
            
            string GetKey(MarkupPolicy markupPolicy, MarkupSubjectInfo subject) 
                => _flow.BuildKey(nameof(DiscountFunctionService), nameof(GetAgentDiscounts), subject.AgencyId.ToString(), subject.AgentId.ToString(), markupPolicy.Id.ToString());

            
            Task<List<Discount>> GetAgentDiscounts()
                => _context.Discounts
                    .Where(d => d.TargetPolicyId == policy.Id)
                    .Where(d => d.TargetAgencyId == subject.AgencyId)
                    .Where(d => d.IsActive)
                    .ToListAsync();
        }


        private static readonly TimeSpan DiscountCacheLifeTime = TimeSpan.FromMinutes(2);
        
        private readonly EdoContext _context;
        private readonly IDoubleFlow _flow;
    }
}