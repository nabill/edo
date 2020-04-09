using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentService : IAgentService
    {
        public AgentService(EdoContext context, IDateTimeProvider dateTimeProvider, IAgentContext agentContext,
            IMarkupPolicyTemplateService markupPolicyTemplateService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _agentContext = agentContext;
            _markupPolicyTemplateService = markupPolicyTemplateService;
        }


        public async Task<Result<Agent>> Add(AgentEditableInfo agentRegistration,
            string externalIdentity,
            string email)
        {
            var (_, isFailure, error) = await Validate(agentRegistration, externalIdentity);
            if (isFailure)
                return Result.Fail<Agent>(error);

            var createdAgent = new Agent
            {
                Title = agentRegistration.Title,
                FirstName = agentRegistration.FirstName,
                LastName = agentRegistration.LastName,
                Position = agentRegistration.Position,
                Email = email,
                IdentityHash = HashGenerator.ComputeSha256(externalIdentity),
                Created = _dateTimeProvider.UtcNow()
            };

            _context.Agents.Add(createdAgent);
            await _context.SaveChangesAsync();

            return Result.Ok(createdAgent);
        }


        public async Task<Result<Agent>> GetMasterAgent(int counterpartyId)
        {
            var master = await (from a in _context.Agents
                join rel in _context.AgentCounterpartyRelations on a.Id equals rel.AgentId
                where rel.CounterpartyId == counterpartyId && rel.Type == AgentCounterpartyRelationTypes.Master
                select a).FirstOrDefaultAsync();

            if (master is null)
                return Result.Fail<Agent>("Master agent does not exist");

            return Result.Ok(master);
        }


        public async Task<AgentEditableInfo> UpdateCurrentAgent(AgentEditableInfo newInfo)
        {
            var currentAgentInfo = await _agentContext.GetAgent();
            var agentToUpdate = await _context.Agents.SingleAsync(a => a.Id == currentAgentInfo.AgentId);

            agentToUpdate.FirstName = newInfo.FirstName;
            agentToUpdate.LastName = newInfo.LastName;
            agentToUpdate.Title = newInfo.Title;
            agentToUpdate.Position = newInfo.Position;

            _context.Agents.Update(agentToUpdate);
            await _context.SaveChangesAsync();

            return newInfo;
        }

        public async Task<Result<List<SlimAgentInfo>>> GetAgents(int counterpartyId, int agencyId = default)
        {
            var currentAgent = await _agentContext.GetAgent();
            var (_, isFailure, error) = CheckCounterpartyAndAgency(currentAgent, counterpartyId, agencyId);
            if (isFailure)
                return Result.Fail<List<SlimAgentInfo>>(error);

            var relations = await
                (from relation in _context.AgentCounterpartyRelations
                join agent in _context.Agents
                    on relation.AgentId equals agent.Id
                join counterparty in _context.Counterparties
                    on relation.CounterpartyId equals counterparty.Id
                join agency in _context.Agencies
                    on relation.AgencyId equals agency.Id
                 where agencyId == default ? relation.CounterpartyId == counterpartyId : relation.AgencyId == agencyId
                 select new {relation, agent, counterparty, agency})
                .ToListAsync();

            var agentIdList = relations.Select(x => x.agent.Id).ToList();

            var markupsMap = (await (
                from markup in _context.MarkupPolicies
                where markup.AgentId != null 
                    && agentIdList.Contains(markup.AgentId.Value)
                    && markup.ScopeType == MarkupPolicyScopeType.Agent
                select markup)
                .ToListAsync())
                .GroupBy(k => (int)k.AgentId)
                .ToDictionary(k => k.Key, v => v.ToList());

            var results = relations.Select(o => 
                new SlimAgentInfo(o.agent.Id, o.agent.FirstName, o.agent.LastName,
                    o.agent.Created, o.counterparty.Id, o.counterparty.Name, o.agency.Id, o.agency.Name,
                    GetMarkupFormula(o.relation)))
                .ToList();

            return Result.Ok(results);

            string GetMarkupFormula(AgentCounterpartyRelation relation)
            {
                if (!markupsMap.TryGetValue(relation.AgentId, out var policies))
                    return string.Empty;
                
                // TODO this needs to be reworked once agencies become ierarchic
                if (currentAgent.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.ObserveMarkupInCounterparty)
                    || currentAgent.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.ObserveMarkupInAgency) && relation.AgencyId == agencyId)
                    return _markupPolicyTemplateService.GetMarkupsFormula(policies);

                return string.Empty;
            }
        }


        public async Task<Result<AgentInfoInAgency>> GetAgent(int counterpartyId, int agencyId, int agentId)
        {
            var agent = await _agentContext.GetAgent();
            var (_, isFailure, error) = CheckCounterpartyAndAgency(agent, counterpartyId, agencyId);
            if (isFailure)
                return Result.Fail<AgentInfoInAgency>(error);

            // TODO this needs to be reworked when agents will be able to belong to more than one agency within a counterparty
            var foundAgent = await (
                    from cr in _context.AgentCounterpartyRelations
                    join a in _context.Agents
                        on cr.AgentId equals a.Id
                    join co in _context.Counterparties
                        on cr.CounterpartyId equals co.Id
                    join ag in _context.Agencies
                        on cr.AgencyId equals ag.Id
                    where (agencyId == default ? cr.CounterpartyId == counterpartyId : cr.AgencyId == agencyId)
                        && cr.AgentId == agentId
                    select (AgentInfoInAgency?) new AgentInfoInAgency(a.Id, a.FirstName, a.LastName, a.Email, a.Title, a.Position, co.Id, co.Name,
                        cr.AgencyId, ag.Name, cr.Type == AgentCounterpartyRelationTypes.Master, cr.InCounterpartyPermissions.ToList()))
                .SingleOrDefaultAsync();

            if (foundAgent == null)
                return Result.Fail<AgentInfoInAgency>("Agent not found in specified counterparty or agency");

            return Result.Ok(foundAgent.Value);
        }


        private Result CheckCounterpartyAndAgency(AgentInfo agent, int counterpartyId, int agencyId)
        {
            if (agent.CounterpartyId != counterpartyId)
                return Result.Fail("The agent isn't affiliated with the counterparty");

            // TODO When agency system gets ierarchic, this needs to be changed so that agent can see agents/markups of his own agency and its subagencies
            if (agencyId != default && agent.AgencyId != agencyId)
                return Result.Fail("The agent isn't affiliated with the agency");

            return Result.Ok();
        }


        private async ValueTask<Result> Validate(AgentEditableInfo agentRegistration, string externalIdentity)
        {
            var fieldValidateResult = GenericValidator<AgentEditableInfo>.Validate(v =>
            {
                v.RuleFor(a => a.Title).NotEmpty();
                v.RuleFor(a => a.FirstName).NotEmpty();
                v.RuleFor(a => a.LastName).NotEmpty();
            }, agentRegistration);

            if (fieldValidateResult.IsFailure)
                return fieldValidateResult;

            return await CheckIdentityIsUnique(externalIdentity);
        }


        private async Task<Result> CheckIdentityIsUnique(string identity)
        {
            return await _context.Agents.AnyAsync(a => a.IdentityHash == HashGenerator.ComputeSha256(identity))
                ? Result.Fail("User is already registered")
                : Result.Ok();
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAgentContext _agentContext;
        private readonly IMarkupPolicyTemplateService _markupPolicyTemplateService;
    }
}