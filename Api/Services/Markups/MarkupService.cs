using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupService : IMarkupService
    {
        public MarkupService(EdoContext context, IMemoryFlow memoryFlow)
        {
            _context = context;
            _memoryFlow = memoryFlow;
        }

        public async Task<Markup> GetMarkup(CustomerInfo customerInfo, MarkupPolicyTarget policyTarget)
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

        private async Task<List<MarkupPolicy>> GetCustomerPolicies(CustomerInfo customerInfo, MarkupPolicyTarget policyTarget)
        {
            var customerId = customerInfo.Customer.Id;
            var companyId = customerInfo.Company.Id;
            var branchId = customerInfo.Branch.Value?.Id;

            return await _context.MarkupPolicies
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
                    () => policy.Function.Compile(),
                    TimeSpan.FromDays(1));
            
            string BuildKey(MarkupPolicy policyWithFunc)
            {
                return _memoryFlow.BuildKey(nameof(MarkupService),
                    "Expressions",
                    policyWithFunc.Id.ToString(),
                    policyWithFunc.Modified.ToString(CultureInfo.InvariantCulture));
            }
        }
        
        private readonly EdoContext _context;
        private readonly IMemoryFlow _memoryFlow;
    }
}