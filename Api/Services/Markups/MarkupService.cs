using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupService : IMarkupService
    {
        public MarkupService(IMarkupPolicyService markupPolicyService,
            ICurrencyRateService currencyRateService, ILogger<MarkupService> logger)
        {
            _markupPolicyService = markupPolicyService;
            _currencyRateService = currencyRateService;
            _logger = logger;
        }


        public async Task<TDetails> ApplyMarkups<TDetails>(MarkupSubjectInfo subject, MarkupDestinationInfo destinationInfo, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc,
            Action<MarkupApplicationResult<TDetails>> logAction = null)
        {
            var firstLevelPercentSum = GetLevelsPercentSum(_markupPolicyService.Get);
            var secondLevelPercentSum = GetLevelsPercentSum(_markupPolicyService.GetSecondLevel);

            PriceProcessFunction percentMarkupFunction = (initialPrice) =>
            {
                // First level markups appliement
                var value = initialPrice.Amount * (100 + firstLevelPercentSum) / 100;
                // Second level markups appliement
                value = value * (100 + secondLevelPercentSum) / 100;
                return new ValueTask<MoneyAmount>(new MoneyAmount(value, initialPrice.Currency));
            };


            return await priceProcessFunc(details, percentMarkupFunction);


            decimal GetLevelsPercentSum(Func<MarkupSubjectInfo, MarkupDestinationInfo, List<MarkupPolicy>> policyGetFunc)
            {
                var policies = policyGetFunc(subject, destinationInfo);

                var percentPolicies = policies
                    .Where(p => p.FunctionType == MarkupFunctionType.Percent)
                    .ToList();

                var percentSum = percentPolicies.Sum(p => p.Value);
                if (percentSum <= 0)
                {
                    var policiesSlim = policies.Select(p => new { Id = p.Id, Value = p.Value }).ToList();
                    _logger.LogMarkupPoliciesSumLessThanZero(subject.AgencyId, percentSum, JsonSerializer.Serialize(policiesSlim));

                    percentSum = 0;
                }

                return percentSum;
            }

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
        private readonly ILogger<MarkupService> _logger;
    }
}