using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Markups.Policies;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupService : IMarkupService
    {
        public MarkupService(EdoContext context)
        {
            _context = context;
        }

        public async Task<Markup> GetMarkup(ICustomerContext customer, MarkupPolicyTarget policyTarget)
        {
            var customerPolicies = await GetCustomerPolicies(customer, policyTarget);
            var markupFunction = CreateMarkupFunction(customerPolicies);
            return new Markup
            {
                Policies = customerPolicies,
                Function = markupFunction
            };
        }

        private async Task<List<MarkupPolicy>> GetCustomerPolicies(ICustomerContext customer, MarkupPolicyTarget policyTarget)
        {
            var customerId = (await customer.GetCustomer()).Value.Id;
            var companyId = (await customer.GetCompany()).Value.Id;

            return await _context.MarkupPolicies
                .Where(p => p.Target == policyTarget)
                .Where(p => p.Scope == MarkupPolicyScope.Global || p.CustomerId == customerId || p.CompanyId == companyId)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        private MarkupFunction CreateMarkupFunction(List<MarkupPolicy> policies)
        {
            var policyEvaluators = policies
                .Select(GetPolicyEvaluator)
                .ToList();
            
            return (supplierPrice, currency) => policyEvaluators
                .Aggregate((decimal)0, (seed, evaluator) => evaluator.Evaluate(seed, currency));
            
            IMarkupPolicyEvaluator GetPolicyEvaluator(MarkupPolicy policy)
            {
                switch (policy.Type)
                {
                    case MarkupPolicyType.Multiplication:
                        return new MultiplyingMarkupPolicy(JsonConvert.DeserializeObject<MultiplyingMarkupSettings>(policy.Settings));
                    case MarkupPolicyType.Addition:
                        return new AdditionMarkupPolicy(JsonConvert.DeserializeObject<AdditionMarkupPolicySettings>(policy.Settings));
                    default:
                        throw new ArgumentException($"Unknown policy type: {policy.Type}");
                }
            }
        }

        private readonly EdoContext _context;
    }
}