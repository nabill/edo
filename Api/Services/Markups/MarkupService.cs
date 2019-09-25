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

        public async Task<Markup> GetMarkup(CustomerData customerData, MarkupPolicyTarget policyTarget)
        {
            // TODO: manage currencies
            var customerPolicies = await GetCustomerPolicies(customerData, policyTarget);
            var markupFunction = CreateAggregatedMarkupFunction(customerPolicies);
            return new Markup
            {
                Policies = customerPolicies,
                Function = markupFunction
            };
        }

        private async Task<List<MarkupPolicy>> GetCustomerPolicies(CustomerData customerData, MarkupPolicyTarget policyTarget)
        {
            var customerId = customerData.Customer.Id;
            var companyId = customerData.Company.Id;

            return await _context.MarkupPolicies
                .Where(p => p.Target == policyTarget)
                .Where(p => 
                    p.ScopeType == MarkupPolicyScopeType.Global ||
                    p.CustomerId == customerId && p.CompanyId == companyId 
                    // TODO: add branches
                    // p.CustomerId == customerId && p.BranchId == companyId ||
                    )
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        private MarkupFunction CreateAggregatedMarkupFunction(List<MarkupPolicy> policies)
        {
            var markupFunctions = policies
                .Select(Compile)
                .ToList();
            
            return supplierPrice => markupFunctions
                .Aggregate((decimal)0, (seed, function) => function(seed));
        }


        private Func<decimal, decimal> Compile(MarkupPolicy policy)
        {
            return _memoryFlow
                .GetOrSet(BuildKey(policy), 
                    () => policy.Function.Compile(),
                    TimeSpan.FromDays(1));
            
            string BuildKey(MarkupPolicy policyWithFunc)
            {
                return _memoryFlow.BuildKey("PolicyExpression",
                    policyWithFunc.Id.ToString(),
                    policyWithFunc.Modified.ToString(CultureInfo.InvariantCulture));
            }
        }
        
        private readonly EdoContext _context;
        private readonly IMemoryFlow _memoryFlow;
    }
}