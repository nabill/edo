using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AgentContextExtensions
    {
        public static bool IsUsingAgency(this AgentContext agentContext, int agencyId) =>
            agentContext.AgencyId == agencyId;


        public static bool IsUsingCounterparty(this AgentContext agentContext, int counterpartyId) =>
            agentContext.CounterpartyId == counterpartyId;
    }
}
