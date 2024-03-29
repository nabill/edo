using System;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums.Markup;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicyScope
    {
        [JsonConstructor]
        public MarkupPolicyScope(SubjectMarkupScopeTypes type, int? agencyId = null, int? agentId = null, string locationId = "")
        {
            Type = type;
            AgencyId = agencyId;
            AgentId = agentId;
            LocationId = locationId;
        }


        public Result Validate()
        {
            return GenericValidator<MarkupPolicyScope>.Validate(v =>
            {
                v.RuleFor(s => s.AgencyId).NotEmpty()
                    .When(t => t.Type == SubjectMarkupScopeTypes.Agency)
                    .WithMessage("AgencyId is required");

                v.RuleFor(s => s.AgencyId).NotEmpty()
                    .When(t => t.Type == SubjectMarkupScopeTypes.Agent)
                    .WithMessage("AgencyId is required");
                v.RuleFor(s => s.AgentId).NotEmpty()
                    .When(t => t.Type == SubjectMarkupScopeTypes.Agent)
                    .WithMessage("AgentId is required");

                v.RuleFor(s => s.LocationId).NotEmpty()
                    .When(t => t.Type == SubjectMarkupScopeTypes.Market || t.Type == SubjectMarkupScopeTypes.Country || t.Type == SubjectMarkupScopeTypes.Locality)
                    .WithMessage("LocationId is required");

                v.RuleFor(s => s.AgencyId).Empty()
                    .When(t => t.Type != SubjectMarkupScopeTypes.Agency && t.Type != SubjectMarkupScopeTypes.Agent)
                    .WithMessage("AgencyId must be empty");

                v.RuleFor(s => s.AgentId).Empty()
                    .When(t => t.Type != SubjectMarkupScopeTypes.Agent)
                    .WithMessage("AgentId must be empty");
            }, this);
        }


        public void Deconstruct(out SubjectMarkupScopeTypes type, out int? agencyId, out int? agentId, out string agentScopeId)
        {
            type = Type;
            agentScopeId = Type switch
            {
                SubjectMarkupScopeTypes.Global => string.Empty,
                SubjectMarkupScopeTypes.Market => LocationId,
                SubjectMarkupScopeTypes.Country => LocationId,
                SubjectMarkupScopeTypes.Locality => LocationId,
                SubjectMarkupScopeTypes.Agency => AgencyId.ToString(),
                SubjectMarkupScopeTypes.Agent => AgentInAgencyId.Create(AgentId.Value, AgencyId.Value).ToString(),
                SubjectMarkupScopeTypes.NotSpecified => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(type), Type, "Wrong AgentMarkupScopeType")
            };
            agencyId = AgencyId;
            agentId = AgentId;
        }


        /// <summary>
        ///     Scope type.
        /// </summary>
        public SubjectMarkupScopeTypes Type { get; }

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