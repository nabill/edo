using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AgentInfoExtensions
    {
        public static bool IsCurrentAgency(this AgentInfo agentInfo, int agencyId) =>
            agentInfo.AgencyId == agencyId;


        public static bool IsCurrentCounterparty(this AgentInfo agentInfo, int counterpartyId) =>
            agentInfo.CounterpartyId == counterpartyId;


        public static Task<bool> IsAffiliatedWithAgency(this AgentInfo agentInfo, EdoContext context, int agencyId) =>
            context.AgentAgencyRelations.AnyAsync(r => r.AgentId == agentInfo.AgentId && r.AgencyId == agencyId);


        public static Task<bool> IsAffiliatedWithCounterparty(this AgentInfo agentInfo, EdoContext context, int counterpartyId) =>
            (from relation in context.AgentAgencyRelations
                join agency in context.Agencies
                    on relation.AgencyId equals agency.Id
                where relation.AgentId == agentInfo.AgentId && agency.CounterpartyId == counterpartyId
                select new object())
                .AnyAsync();
    }
}
