using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters
{
    public class MinCounterpartyStateAuthorizationRequirement : IAuthorizationRequirement
    {
        public MinCounterpartyStateAuthorizationRequirement(CounterpartyStates counterpartyState)
        {
            CounterpartyState = counterpartyState;
        }
        
        public CounterpartyStates CounterpartyState { get; }
    }
}