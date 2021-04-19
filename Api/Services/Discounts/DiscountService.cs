using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Discounts
{
    public class DiscountService : IDiscountService
    {
        public DiscountService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }
        
        
        public async Task<TDetails> ApplyDiscounts<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Action<DiscountApplicationResult<TDetails>> logAction)
        {
            var discountKey = GetKey(agent);
            var applicableDiscounts = await _flow.GetOrSetAsync(discountKey, GetAgentDiscounts, DiscountCacheLifeTime);

            var currentDetails = details;
            foreach (var discount in applicableDiscounts)
            {
                var detailsBefore = currentDetails;
                var function = GetDiscountFunction(discount);
                currentDetails = await priceProcessFunc(detailsBefore, function);

                logAction?.Invoke(new DiscountApplicationResult<TDetails>
                {
                    After = currentDetails,
                    Before = detailsBefore,
                    Discount = discount
                });
            }

            var ceiledResponse = await priceProcessFunc(currentDetails, price =>
                new ValueTask<MoneyAmount>(MoneyRounder.Ceil(price)));

            return ceiledResponse;


            string GetKey(AgentContext agentContext) 
                => _flow.BuildKey(nameof(DiscountService), nameof(GetAgentDiscounts), agentContext.AgencyId.ToString(), agentContext.AgentId.ToString());

            
            Task<List<Discount>> GetAgentDiscounts()
                => _context.Discounts
                    .Where(d => d.TargetAgencyId == agent.AgencyId)
                    .Where(d => d.IsActive)
                    .ToListAsync();
            
            
            static PriceProcessFunction GetDiscountFunction(Discount discount)
                => moneyAmount =>
                {
                    var processedAmount = new MoneyAmount
                    {
                        Amount = moneyAmount.Amount * (100 - discount.DiscountPercent) / 100,
                        Currency = moneyAmount.Currency
                    };
                    return new ValueTask<MoneyAmount>(processedAmount);
                };
        }
        
        
        private static readonly TimeSpan DiscountCacheLifeTime = TimeSpan.FromMinutes(2);
        
        private readonly EdoContext _context;
        private readonly IDoubleFlow _flow;
    }
}