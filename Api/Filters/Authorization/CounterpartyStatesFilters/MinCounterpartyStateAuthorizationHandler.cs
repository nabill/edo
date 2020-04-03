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

namespace HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters
{
    public class MinCounterpartyStateAuthorizationHandler : AuthorizationHandler<MinCounterpartyStateAuthorizationRequirement>
    {
        public MinCounterpartyStateAuthorizationHandler(ICustomerContextInternal customerContextInternal, IMemoryFlow flow,
            EdoContext context, ILogger<MinCounterpartyStateAuthorizationHandler> logger)
        {
            _customerContextInternal = customerContextInternal;
            _flow = flow;
            _context = context;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MinCounterpartyStateAuthorizationRequirement requirement)
        {
            var (_, isCustomerFailure, customer, customerError) = await _customerContextInternal.GetCustomerInfo();
            if (isCustomerFailure)
            {
                _logger.LogCustomerFailedToAuthorize($"Could not find customer: '{customerError}'");
                context.Fail();
                return;
            }
            
            var counterpartyState = await GetCounterpartyState(customer.CounterpartyId);

            switch (counterpartyState)
            {
                case CounterpartyStates.FullAccess:
                    context.Succeed(requirement);
                    _logger.LogCounterpartyStateChecked($"Successfully checked counterparty state for customer {customer.Email}");
                    return;
                
                case CounterpartyStates.ReadOnly:
                    if (requirement.CounterpartyState == CounterpartyStates.ReadOnly)
                    {
                        context.Succeed(requirement);
                        _logger.LogCounterpartyStateChecked($"Successfully checked counterparty state for customer {customer.Email}");
                    }
                    else
                    {
                        _logger.LogCounterpartyStateCheckFailed($"Counterparty of customer '{customer.Email}' has wrong state." +
                            $" Expected '{CounterpartyStates.ReadOnly}' or '{CounterpartyStates.FullAccess}' but was '{counterpartyState}'");
                        context.Fail();
                    }

                    return;

                default:
                    _logger.LogCounterpartyStateCheckFailed($"Counterparty of customer '{customer.Email}' has wrong state: '{counterpartyState}'");
                    context.Fail();
                    return;
            }


            ValueTask<CounterpartyStates> GetCounterpartyState(int counterpartyId)
            {
                var cacheKey = _flow.BuildKey(nameof(MinCounterpartyStateAuthorizationHandler), nameof(GetCounterpartyState), counterpartyId.ToString());
                return _flow.GetOrSetAsync(cacheKey, ()
                        => _context.Counterparties
                            .Where(c => c.Id == counterpartyId)
                            .Select(c => c.State)
                            .SingleOrDefaultAsync(),
                    CounterpartyStateCacheTtl);
            }
        }


        private static readonly TimeSpan CounterpartyStateCacheTtl = TimeSpan.FromMinutes(5);
        private readonly EdoContext _context;
        private readonly ILogger<MinCounterpartyStateAuthorizationHandler> _logger;
        private readonly ICustomerContextInternal _customerContextInternal;
        private readonly IMemoryFlow _flow;
    }
}