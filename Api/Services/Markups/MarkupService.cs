using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupService : IMarkupService
    {
        public MarkupService(EdoContext context, IMemoryFlow memoryFlow,
            IMarkupPolicyTemplateService templateService,
            ICurrencyRateService currencyRateService,
            IAgentSettingsManager agentSettingsManager)
        {
            _context = context;
            _memoryFlow = memoryFlow;
            _templateService = templateService;
            _currencyRateService = currencyRateService;
            _agentSettingsManager = agentSettingsManager;
        }


        public async Task<Markup> Get(AgentInfo agentInfo, MarkupPolicyTarget policyTarget)
        {
            var settings = await _agentSettingsManager.GetUserSettings(agentInfo);
            var agentPolicies = await GetAgentPolicies(agentInfo, settings, policyTarget);
            var markupFunction = CreateAggregatedMarkupFunction(agentPolicies);
            return new Markup
            {
                Policies = agentPolicies,
                Function = markupFunction
            };
        }


        private ValueTask<List<MarkupPolicy>> GetAgentPolicies(AgentInfo agentInfo, AgentUserSettings userSettings,
            MarkupPolicyTarget policyTarget)
        {
            var (agentId, counterpartyId, agencyId, _) = agentInfo;

            return _memoryFlow.GetOrSetAsync(BuildKey(),
                GetOrderedPolicies,
                AgentPoliciesCachingTime);


            string BuildKey()
                => _memoryFlow.BuildKey(nameof(MarkupService),
                    "MarkupPolicies",
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


        private PriceProcessFunction CreateAggregatedMarkupFunction(List<MarkupPolicy> policies)
        {
            var markupPolicyFunctions = policies
                .Select(GetPolicyFunction)
                .ToList();

            // TODO: rewrite to async streams after migrating to .NET Core 3
            return async (supplierPrice, currency) =>
            {
                var price = supplierPrice;
                foreach (var markupPolicyFunction in markupPolicyFunctions)
                {
                    var (_, _, currencyRate, _) = await _currencyRateService.Get(currency, markupPolicyFunction.Currency);
                    price = markupPolicyFunction.Function(price * currencyRate) / currencyRate;
                }

                return (price, currency);
            };
        }


        private MarkupPolicyFunction GetPolicyFunction(MarkupPolicy policy)
        {
            return _memoryFlow
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
                => _memoryFlow.BuildKey(nameof(MarkupService),
                    "Functions",
                    policyWithFunc.Id.ToString(),
                    policyWithFunc.Modified.ToString(CultureInfo.InvariantCulture));
        }


        private static readonly TimeSpan MarkupPolicyFunctionCachingTime = TimeSpan.FromDays(1);
        private static readonly TimeSpan AgentPoliciesCachingTime = TimeSpan.FromMinutes(5);
        private readonly EdoContext _context;
        private readonly ICurrencyRateService _currencyRateService;
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly IMemoryFlow _memoryFlow;
        private readonly IMarkupPolicyTemplateService _templateService;
    }
}