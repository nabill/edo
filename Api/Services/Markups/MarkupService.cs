using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupService : IMarkupService
    {
        public MarkupService(IMarkupPolicyService markupPolicyService,
            ICurrencyRateService currencyRateService)
        {
            _markupPolicyService = markupPolicyService;
            _currencyRateService = currencyRateService;
        }
        
        
        public async Task<TDetails> ApplyMarkups<TDetails>(MarkupSubjectInfo subject, MarkupDestinationInfo destinationInfo, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Action<MarkupApplicationResult<TDetails>> logAction = null)
        {
            var policies = _markupPolicyService.Get(subject, destinationInfo);
            
            // Addition markups are not applied for now
            var percentPolicies = policies
                .Where(p => p.FunctionType == MarkupFunctionType.Percent)
                .ToList();

            var percentSum = percentPolicies.Sum(p => p.Value);
            if (percentSum < 0)
                throw new ArgumentException("Invalid markup settings");
            
            PriceProcessFunction percentMarkupFunction = (initialPrice) =>
            {
                var value = initialPrice.Amount * (100 + percentSum) / 100;
                return new ValueTask<MoneyAmount>(new MoneyAmount(value, initialPrice.Currency));
            };
            
            
            return await priceProcessFunc(details, percentMarkupFunction);
            
            // TODO: Restore markup bonuses addition https://github.com/happy-travel/agent-app-project/issues/1242
            // var currentData = details;
            //
            //
            // decimal percents = 0;
            // foreach (var policy in percentPolicies)
            // {
            //     var markupPercent = policy.Value;
            //     percents += markupPercent;
            //     var detailsBefore = currentData;
            //     
            //     var markupFunction = GetPriceProcessFunction(policy);
            //     currentData = await priceProcessFunc(currentData, markupFunction);
            //
            //     logAction?.Invoke(new MarkupApplicationResult<TDetails>(detailsBefore, policy, currentData));
            // }
            //
            // currentData = await priceProcessFunc(currentData, percentMarkupFunction);
            //
            // return currentData;
        }


        private readonly ICurrencyRateService _currencyRateService;
        
        private readonly IMarkupPolicyService _markupPolicyService;
    }
}