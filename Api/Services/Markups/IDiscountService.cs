using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IDiscountService
    {
        Task<TDetails> ApplyDiscounts<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Action<DiscountApplicationResult<TDetails>> logAction = null);
    }
    

    public readonly struct DiscountApplicationResult<TDetails>
    {
        public TDetails Before { get; init; }
        public Discount Discount { get; init; }
        public TDetails After { get; init; }
    }
}