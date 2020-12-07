using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupFunctionService : IMarkupFunctionService
    {
        public MarkupFunctionService(EdoContext context, 
            IDoubleFlow flow,
            IMarkupPolicyTemplateService templateService,
            ICurrencyRateService currencyRateService,
            IAgentSettingsManager agentSettingsManager,
            IAccommodationBookingSettingsService accommodationBookingSettingsService)
        {
            _context = context;
            _flow = flow;
            _templateService = templateService;
            _currencyRateService = currencyRateService;
            _agentSettingsManager = agentSettingsManager;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
        }


        public async Task<List<(MarkupPolicy Policy, PriceProcessFunction Function)>> GetFunctions(AgentContext agentContext, MarkupPolicyTarget policyTarget)
        {
            var searchSettings = await _accommodationBookingSettingsService.Get(agentContext);
            if (searchSettings.IsMarkupDisabled)
                return new List<(MarkupPolicy, PriceProcessFunction)>(0);
            
            var agentSettings = await GetAgentSettings(agentContext);
            var agentPolicies = await GetAgentPolicies(agentContext, agentSettings, policyTarget);
            return CreateAggregatedMarkupFunction(agentPolicies);
        }


        private Task<AgentUserSettings> GetAgentSettings(AgentContext agentContext)
        {
            return _flow.GetOrSetAsync(
                key: BuildKey(),
                getValueFunction: async () => await _agentSettingsManager.GetUserSettings(agentContext),
                AgentSettingsCachingTime); 
            
            string BuildKey()
                => _flow.BuildKey(nameof(MarkupFunctionService),
                    nameof(GetAgentSettings),
                    agentContext.AgentId.ToString());
        }


        private Task<List<MarkupPolicy>> GetAgentPolicies(AgentContext agentContext, AgentUserSettings userSettings,
            MarkupPolicyTarget policyTarget)
        {
            var (agentId, counterpartyId, agencyId, _) = agentContext;

            return _flow.GetOrSetAsync(BuildKey(),
                GetOrderedPolicies,
                AgentPoliciesCachingTime);


            string BuildKey()
                => _flow.BuildKey(nameof(MarkupFunctionService),
                    nameof(GetAgentPolicies),
                    agentId.ToString());


            async Task<List<MarkupPolicy>> GetOrderedPolicies()
            {
                var policiesFromDb = await GetPoliciesFromDb();
                return policiesFromDb.Where(FilterBySettings)
                    .OrderBy(SortByScope)
                    .ThenBy(p => p.Order)
                    .ToList();
            }


            bool FilterBySettings(MarkupPolicy policy)
            {
                if (policy.ScopeType == MarkupPolicyScopeType.EndClient && !userSettings.IsEndClientMarkupsEnabled)
                    return false;

                return true;
            }


            int SortByScope(MarkupPolicy policy) => (int) policy.ScopeType;


            Task<List<MarkupPolicy>> GetPoliciesFromDb()
            {
                return _context.MarkupPolicies
                    .Where(p => p.Target == policyTarget)
                    .Where(p =>
                        p.ScopeType == MarkupPolicyScopeType.Global ||
                        p.ScopeType == MarkupPolicyScopeType.Counterparty && p.CounterpartyId == counterpartyId ||
                        p.ScopeType == MarkupPolicyScopeType.Agency && p.AgencyId == agencyId ||
                        p.ScopeType == MarkupPolicyScopeType.Agent && p.AgentId == agentId ||
                        p.ScopeType == MarkupPolicyScopeType.EndClient && p.AgentId == agentId
                    )
                    .ToListAsync();
            }
        }


        private List<(MarkupPolicy, PriceProcessFunction)> CreateAggregatedMarkupFunction(List<MarkupPolicy> policies)
        {
            return policies
                .Select(policy =>
                {
                    var policyFunction = GetPolicyFunction(policy);
                    PriceProcessFunction processFunction = async initialPrice =>
                    {
                        var amount = initialPrice.Amount;
                        var (_, _, currencyRate, _) = await _currencyRateService.Get(initialPrice.Currency, policyFunction.Currency);
                        amount = policyFunction.Function(amount * currencyRate) / currencyRate;
                        return new MoneyAmount(amount, initialPrice.Currency);
                    };
                    return (policy, processFunction);
                })
                .ToList();
        }


        private MarkupPolicyFunction GetPolicyFunction(MarkupPolicy policy)
        {
            return _flow
                .GetOrSet(BuildKey(policy),
                    () =>
                    {
                        return new MarkupPolicyFunction
                        {
                            Currency = policy.Currency,
                            Function = _templateService
                                .CreateFunction(policy.TemplateId, policy.TemplateSettings)
                        };
                    },
                    MarkupPolicyFunctionCachingTime);


            string BuildKey(MarkupPolicy policyWithFunc)
                => _flow.BuildKey(nameof(MarkupFunctionService),
                    nameof(GetPolicyFunction),
                    policyWithFunc.Id.ToString(),
                    policyWithFunc.Modified.ToString(CultureInfo.InvariantCulture));
        }


        private static readonly TimeSpan MarkupPolicyFunctionCachingTime = TimeSpan.FromDays(1);
        private static readonly TimeSpan AgentPoliciesCachingTime = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan AgentSettingsCachingTime = TimeSpan.FromMinutes(2);
        private readonly EdoContext _context;
        private readonly ICurrencyRateService _currencyRateService;
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IDoubleFlow _flow;
        private readonly IMarkupPolicyTemplateService _templateService;
    }
}