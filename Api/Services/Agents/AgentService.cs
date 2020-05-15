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


        public async Task<Result<Agent>> GetMasterAgent(int agencyId)
        {
            var master = await (from a in _context.Agents
                join rel in _context.AgentCounterpartyRelations on a.Id equals rel.AgentId
                where rel.AgencyId == agencyId && rel.Type == AgentAgencyRelationTypes.Master
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

        public async Task<Result<List<SlimAgentInfo>>> GetAgents(int agencyId)
        {
            var currentAgent = await _agentContext.GetAgent();

            var relations = await
                (from relation in _context.AgentCounterpartyRelations
                join agent in _context.Agents
                    on relation.AgentId equals agent.Id
                join agency in _context.Agencies
                    on relation.AgencyId equals agency.Id
                join counterparty in _context.Counterparties
                    on agency.CounterpartyId equals counterparty.Id
                 where relation.AgencyId == agencyId
                 select new {relation, agent, agency, counterparty})
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
                if (currentAgent.InAgencyPermissions.HasFlag(InAgencyPermissions.ObserveMarkupInCounterparty)
                    || currentAgent.InAgencyPermissions.HasFlag(InAgencyPermissions.ObserveMarkupInAgency) && relation.AgencyId == agencyId)
                    return _markupPolicyTemplateService.GetMarkupsFormula(policies);

                return string.Empty;
            }
        }


        public async Task<Result<AgentInfoInAgency>> GetAgent(int agencyId, int agentId)
        {
            var foundAgent = await (
                    from cr in _context.AgentCounterpartyRelations
                    join a in _context.Agents
                        on cr.AgentId equals a.Id
                    join ag in _context.Agencies
                        on cr.AgencyId equals ag.Id
                    join co in _context.Counterparties
                        on ag.CounterpartyId equals co.Id
                    where cr.AgencyId == agencyId && cr.AgentId == agentId
                    select (AgentInfoInAgency?) new AgentInfoInAgency(a.Id, a.FirstName, a.LastName, a.Email, a.Title, a.Position, co.Id, co.Name,
                        cr.AgencyId, ag.Name, cr.Type == AgentAgencyRelationTypes.Master, cr.InAgencyPermissions.ToList()))
                .SingleOrDefaultAsync();

            if (foundAgent == null)
                return Result.Fail<AgentInfoInAgency>("Agent not found in specified counterparty or agency");

            return Result.Ok(foundAgent.Value);
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