using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupService : IMarkupService
    {
        public MarkupService(EdoContext context, IMemoryFlow memoryFlow, IMarkupPolicyTemplateService templateService)
        {
            _context = context;
            _memoryFlow = memoryFlow;
            _templateService = templateService;
        }

        public async Task<Markup> Get(CustomerInfo customerInfo, MarkupPolicyTarget policyTarget)
        {
            // TODO: manage currencies
            var customerPolicies = await GetCustomerPolicies(customerInfo, policyTarget);
            var markupFunction = CreateAggregatedMarkupFunction(customerPolicies);
            return new Markup
            {
                Policies = customerPolicies,
                Function = markupFunction
            };
        }

        private ValueTask<List<MarkupPolicy>> GetCustomerPolicies(CustomerInfo customerInfo, MarkupPolicyTarget policyTarget)
        {
            var customerId = customerInfo.Customer.Id;
            var companyId = customerInfo.Company.Id;
            var branchId = customerInfo.Branch.Value?.Id;

            return _memoryFlow.GetOrSetAsync(BuildKey(),
                GetPoliciesFromDb,
                CustomerPoliciesCachingTime);

            string BuildKey()
            {
                return _memoryFlow.BuildKey(nameof(MarkupService),
                    "MarkupPolicies",
                    customerId.ToString());
            }

            Task<List<MarkupPolicy>> GetPoliciesFromDb()
            {
                return _context.MarkupPolicies
                    .Where(p => p.Target == policyTarget)
                    .Where(p => 
                        p.ScopeType == MarkupPolicyScopeType.Global ||
                        (p.ScopeType == MarkupPolicyScopeType.Company && p.CompanyId == companyId) ||
                        (p.ScopeType == MarkupPolicyScopeType.Branch && p.BranchId == branchId) ||
                        (p.ScopeType == MarkupPolicyScopeType.Customer && p.CustomerId == customerId) 
                    )
                    .OrderBy(p => p.Order)
                    .ToListAsync();
            }
        }

        private MarkupFunction CreateAggregatedMarkupFunction(List<MarkupPolicy> policies)
        {
            var markupFunctions = policies
                .Select(GetFunction)
                .ToList();
            
            return supplierPrice => markupFunctions
                .Aggregate((decimal)0, (seed, function) => function(seed));
        }


        private Func<decimal, decimal> GetFunction(MarkupPolicy policy)
        {
            return _memoryFlow
                .GetOrSet(BuildKey(policy), 
                    () => _templateService
                        .CreateFunction(policy.TemplateId, policy.TemplateSettings),
                    MarkupFunctionCachingTime);
            
            string BuildKey(MarkupPolicy policyWithFunc)
            {
                return _memoryFlow.BuildKey(nameof(MarkupService),
                    "Functions",
                    policyWithFunc.Id.ToString(),
                    policyWithFunc.Modified.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static readonly TimeSpan MarkupFunctionCachingTime = TimeSpan.FromDays(1);
        private static readonly TimeSpan CustomerPoliciesCachingTime = TimeSpan.FromMinutes(5);
        private readonly EdoContext _context;
        private readonly IMemoryFlow _memoryFlow;
        private readonly IMarkupPolicyTemplateService _templateService;
    }
}