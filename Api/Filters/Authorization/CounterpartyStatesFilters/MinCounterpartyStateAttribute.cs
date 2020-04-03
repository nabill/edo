using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters
{
    public class MinCounterpartyStateAttribute : AuthorizeAttribute
    {
        public MinCounterpartyStateAttribute(CounterpartyStates minimalState)
        {
            Policy = $"{PolicyPrefix}{minimalState}";
        }
        
        public const string PolicyPrefix = "MinCounterpartyState_";
    }
}