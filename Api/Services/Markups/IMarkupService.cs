using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.PriceProcessing;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupService
    {
        Task<TDetails> ApplyMarkups<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc, Action<MarkupApplicationResult<TDetails>> logAction = null);
    }
}