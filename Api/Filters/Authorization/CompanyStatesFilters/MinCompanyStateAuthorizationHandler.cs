using System;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.CompanyStatesFilters
{
    public class MinCompanyStateAuthorizationHandler : AuthorizationHandler<MinCompanyStateAuthorizationRequirement>
    {
        public MinCompanyStateAuthorizationHandler(ICustomerContext customerContext, IMemoryFlow flow,
            EdoContext context, ILogger<MinCompanyStateAuthorizationHandler> logger)
        {
            _customerContext = customerContext;
            _flow = flow;
            _context = context;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MinCompanyStateAuthorizationRequirement requirement)
        {
            var customer = await _customerContext.GetCustomer();
            var companyState = await GetCompanyState(customer.CompanyId);

            switch (companyState)
            {
                case CompanyStates.FullAccess:
                    context.Succeed(requirement);
                    _logger.LogCompanyStateChecked($"Successfully checked company state for customer {customer.Email}");
                    return;
                
                case CompanyStates.ReadOnly:
                    if (requirement.CompanyState == CompanyStates.ReadOnly)
                    {
                        context.Succeed(requirement);
                        _logger.LogCompanyStateChecked($"Successfully checked company state for customer {customer.Email}");
                    }
                    else
                    {
                        _logger.LogCompanyStateCheckFailed($"Company of customer '{customer.Email}' has wrong state." +
                            $" Expected '{CompanyStates.ReadOnly}' or '{CompanyStates.FullAccess}' but was '{companyState}'");
                        context.Fail();
                    }

                    return;

                default:
                    _logger.LogCompanyStateCheckFailed($"Company of customer '{customer.Email}' has wrong state: '{companyState}'");
                    context.Fail();
                    return;
            }


            ValueTask<CompanyStates> GetCompanyState(int companyId)
            {
                var cacheKey = _flow.BuildKey(nameof(MinCompanyStateAuthorizationHandler), nameof(GetCompanyState), companyId.ToString());
                return _flow.GetOrSetAsync(cacheKey, ()
                        => _context.Companies
                            .Where(c => c.Id == companyId)
                            .Select(c => c.State)
                            .SingleOrDefaultAsync(),
                    CompanyStateCacheTtl);
            }
        }


        private static readonly TimeSpan CompanyStateCacheTtl = TimeSpan.FromMinutes(5);
        private readonly EdoContext _context;
        private readonly ILogger<MinCompanyStateAuthorizationHandler> _logger;
        private readonly ICustomerContext _customerContext;
        private readonly IMemoryFlow _flow;
    }
}