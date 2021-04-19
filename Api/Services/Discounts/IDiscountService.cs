using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.PriceProcessing;

namespace HappyTravel.Edo.Api.Services.Discounts
{
    public interface IDiscountService
    {
        Task<TDetails> ApplyDiscounts<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Action<DiscountApplicationResult<TDetails>> logAction = null);
    }
}