using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupService : IMarkupService
    {
        public MarkupService(IMarkupFunctionService markupFunctionService)
        {
            _markupFunctionService = markupFunctionService;
        }
        
        
        public async Task<TDetails> ApplyMarkups<TDetails>(AgentContext agent, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc, 
            Action<MarkupApplicationResult<TDetails>> logAction = null)
        {
            // Getting separate function for each applicable policy
            var functions = await _markupFunctionService.GetFunctions(agent, MarkupPolicyTarget.AccommodationAvailability);
            var currentData = details;
            foreach (var function in functions)
            {
                var detailsBefore = currentData;
                currentData = await priceProcessFunc(currentData, function.Function);
                // Executing action to make outer service know what was happened
                logAction?.Invoke(new MarkupApplicationResult<TDetails>
                {
                    Before = detailsBefore,
                    Policy = function.Policy,
                    After = currentData
                });
            }
            
            var ceiledResponse = await priceProcessFunc(currentData, price =>
                new ValueTask<MoneyAmount>(MoneyRounder.Ceil(price)));

            return ceiledResponse;
        }
        
        
        private readonly IMarkupFunctionService _markupFunctionService;
    }
}