using System;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums.Markup;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicyScope
    {
        [JsonConstructor]
        public MarkupPolicyScope(AgentMarkupScopeTypes type, int? counterpartyId = null, int? agencyId = null, int? agentId = null, string locationId = "")
        {
            Type = type;
            CounterpartyId = counterpartyId;
            AgencyId = agencyId;
            AgentId = agentId;
            LocationId = locationId;
        }


        public Result Validate()
        {
            return GenericValidator<MarkupPolicyScope>.Validate(v =>
            {
                v.RuleFor(s => s.CounterpartyId).NotEmpty()
                    .When(t => t.Type == AgentMarkupScopeTypes.Counterparty)
                    .WithMessage("CounterpartyId is required");
                v.RuleFor(s => s.AgencyId).NotEmpty()
                    .When(t => t.Type == AgentMarkupScopeTypes.Agency)
                    .WithMessage("AgencyId is required");
                
                v.RuleFor(s => s.AgencyId).NotEmpty()
                    .When(t => t.Type == AgentMarkupScopeTypes.Agent)
                    .WithMessage("AgencyId is required");
                v.RuleFor(s => s.AgentId).NotEmpty()
                    .When(t => t.Type == AgentMarkupScopeTypes.Agent)
                    .WithMessage("AgentId is required");
                
                v.RuleFor(s => s.LocationId).NotEmpty()
                    .When(t => t.Type == AgentMarkupScopeTypes.Location)
                    .WithMessage("LocationId is required");

                v.RuleFor(s => s.CounterpartyId).Empty()
                    .When(t => t.Type != AgentMarkupScopeTypes.Counterparty)
                    .WithMessage("CounterpartyId must be empty");
                
                v.RuleFor(s => s.AgencyId).Empty()
                    .When(t => t.Type != AgentMarkupScopeTypes.Agency && t.Type != AgentMarkupScopeTypes.Agent)
                    .WithMessage("AgencyId must be empty");
                
                v.RuleFor(s => s.AgentId).Empty()
                    .When(t => t.Type != AgentMarkupScopeTypes.Agent)
                    .WithMessage("AgentId must be empty");
            }, this);
        }


        public void Deconstruct(out AgentMarkupScopeTypes type, out string agentScopeId)
        {
            type = Type;
            agentScopeId = type switch
            {
                AgentMarkupScopeTypes.Global => "",
                AgentMarkupScopeTypes.Counterparty => CounterpartyId.ToString(),
                AgentMarkupScopeTypes.Agency => AgencyId.ToString(),
                AgentMarkupScopeTypes.Agent => $"{AgencyId}-{AgentId}",
                AgentMarkupScopeTypes.Location => LocationId,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Wrong AgentMarkupScopeType")
            };
        }
        

        /// <summary>
        ///     Scope type.
        /// </summary>
        public AgentMarkupScopeTypes Type { get; }

        /// <summary>
        ///     Counterparty id for counterparty scope type
        /// </summary>
        public int? CounterpartyId { get; }

        /// <summary>
        ///     Agency id for agency scope type or agent scope type
        /// </summary>
        public int? AgencyId { get; }

        /// <summary>
        ///     Agent id for agent scope type
        /// </summary>
        public int? AgentId { get; }
        
        /// <summary>
        ///     Location id for location scope type
        /// </summary>
        public string LocationId { get; }
    }
}