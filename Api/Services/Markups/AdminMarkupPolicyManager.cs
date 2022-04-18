using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agency;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Markups.Global;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class AdminMarkupPolicyManager : IAdminMarkupPolicyManager
    {
        public AdminMarkupPolicyManager(EdoContext context,
            IMarkupPolicyTemplateService templateService,
            IDateTimeProvider dateTimeProvider,
            IDisplayedMarkupFormulaService displayedMarkupFormulaService,
            IAdministratorContext administratorContext,
            IMarkupPolicyAuditService markupPolicyAuditService,
            IAccommodationMapperClient mapperClient)
        {
            _context = context;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;
            _displayedMarkupFormulaService = displayedMarkupFormulaService;
            _administratorContext = administratorContext;
            _markupPolicyAuditService = markupPolicyAuditService;
            _mapperClient = mapperClient;
        }


        public async Task<GlobalMarkupInfo?> GetGlobalPolicy()
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global)
                .SingleOrDefaultAsync();

            return policy is null
                ? null
                : new GlobalMarkupInfo { Percent = policy.Value };
        }


        public async Task<Result> RemoveGlobalPolicy()
        {
            var policy = await _context.MarkupPolicies
                .SingleOrDefaultAsync(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global);

            return policy is null
                ? Result.Failure("Could not find global policy")
                : await Remove(policy.Id);
        }


        public async Task<Result> AddGlobalPolicy(SetGlobalMarkupRequest request)
        {
            var policy = await _context.MarkupPolicies
                .SingleOrDefaultAsync(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global);

            var settings = new MarkupPolicySettings("Global markup", MarkupFunctionType.Percent,
                request.Percent, Currencies.USD, null);

            if (settings.Value <= 0)
                return Result.Failure("Global markup policy must have positive value");

            if (policy is null)
            {
                var policyData = new MarkupPolicyData(settings,
                    new MarkupPolicyScope(SubjectMarkupScopeTypes.Global));

                return await Add(policyData);
            }

            return await Update(policy.Id, settings);
        }


        public Task<List<MarkupInfo>> GetLocationPolicies()
            => _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Market ||
                    p.SubjectScopeType == SubjectMarkupScopeTypes.Country ||
                    p.SubjectScopeType == SubjectMarkupScopeTypes.Locality ||
                    p.DestinationScopeType == DestinationMarkupScopeTypes.Market ||
                    p.DestinationScopeType == DestinationMarkupScopeTypes.Country ||
                    p.DestinationScopeType == DestinationMarkupScopeTypes.Locality)
                .OrderBy(p => p.FunctionType)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();


        public async Task<Result> AddLocationPolicy(MarkupPolicySettings settings)
        {
            return await ValidateAddLocation(settings)
                .TapIf(settings.LocationScopeType is not null, async () => await AddPolicy())
                .TapIf(settings.DestinationScopeType is not null, async () => await AddDestinationPolicy());


            async Task<Result> AddPolicy()
            {
                var (_, isFailure, agentMarkupScopeType, error) = await GetAgentMarkupScopeType(settings);
                if (isFailure)
                    return Result.Failure(error);

                return await Add(new MarkupPolicyData(settings,
                    new MarkupPolicyScope(agentMarkupScopeType, locationId: settings.LocationScopeId ?? string.Empty)));
            }


            async Task<Result> AddDestinationPolicy()
                => await Add(new MarkupPolicyData(settings,
                    new MarkupPolicyScope(SubjectMarkupScopeTypes.Global, locationId: settings.LocationScopeId ?? string.Empty)));


            Result ValidateAddLocation(MarkupPolicySettings settings)
                => GenericValidator<MarkupPolicySettings>.Validate(v =>
                    {
                        var valueValidatorMessage = "Markup policy value must be in range (-100..-0.1) or (0.1..100)";

                        v.RuleFor(s => Math.Abs(s.Value))
                            .GreaterThanOrEqualTo(0.1m)
                            .WithMessage(valueValidatorMessage)
                            .LessThanOrEqualTo(100m)
                            .WithMessage(valueValidatorMessage);

                        v.When(m => m.LocationScopeId is null || m.LocationScopeType is null, () =>
                        {
                            v.RuleFor(m => m.DestinationScopeId)
                                .NotNull()
                                .MustAsync(MarketMarkupIsNotExist()!)
                                .When(m => m.DestinationScopeType == DestinationMarkupScopeTypes.Market)
                                .WithMessage(m => $"Destination markup policy with DestinationScopeId {m.DestinationScopeId} already exists or unexpected value!"); ;

                            v.RuleFor(m => m.DestinationScopeType)
                                .NotNull()
                                .Must(d => d.Equals(DestinationMarkupScopeTypes.Market) || d.Equals(DestinationMarkupScopeTypes.Country) ||
                                    d.Equals(DestinationMarkupScopeTypes.Locality))
                                .WithMessage($"Request's destinationScopeType must be Market,Country or Locality");
                        }).Otherwise(() =>
                        {
                            v.RuleFor(s => s.DestinationScopeId)
                                .Null();

                            v.RuleFor(s => s.DestinationScopeType)
                                .Null();

                            v.RuleFor(m => m.LocationScopeId)
                                .MustAsync(MarketMarkupIsNotExist()!)
                                .When(m => m.LocationScopeType == SubjectMarkupScopeTypes.Market)
                                .WithMessage(m => $"Location markup policy with LocationScopeId {m.LocationScopeId} already exists or unexpected value!");

                            v.RuleFor(m => m.LocationScopeType)
                                .Must(d => d.Equals(SubjectMarkupScopeTypes.Market) || d.Equals(SubjectMarkupScopeTypes.Country) ||
                                    d.Equals(SubjectMarkupScopeTypes.Locality))
                                .WithMessage($"Request's locationScopeType must be Market,Country or Locality");
                        });
                    }, settings);


            System.Func<string, System.Threading.CancellationToken, Task<bool>> MarketMarkupIsNotExist()
            => async (marketId, cancelationToken)
                => !(await _context.MarkupPolicies.AnyAsync(m => m.DestinationScopeId == marketId, cancelationToken)) &&
                    await _context.Markets.AnyAsync(m => m.Id.ToString() == marketId, cancelationToken);
        }


        public async Task<Result> ModifyLocationPolicy(int policyId, MarkupPolicySettings settings)
        {
            var policy = await _context.MarkupPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId);

            return await ValidateModifyLocation((settings, policy))
                .Tap(UpdatePolicy);


            async Task<Result> UpdatePolicy()
                => await Update(policyId, settings);


            Result ValidateModifyLocation((MarkupPolicySettings settings, MarkupPolicy? policy) entity)
                => GenericValidator<(MarkupPolicySettings settings, MarkupPolicy? policy)>.Validate(v =>
                    {
                        var valueValidatorMessage = "Markup policy value must be in range (-100..-0.1) or (0.1..100)";

                        v.RuleFor(t => Math.Abs(t.settings.Value))
                            .GreaterThanOrEqualTo(0.1m)
                            .WithMessage(valueValidatorMessage)
                            .LessThanOrEqualTo(100m)
                            .WithMessage(valueValidatorMessage);

                        v.RuleFor(t => t.policy)
                            .NotNull()
                            .WithMessage($"Markup policy with Id {policyId} was not found!");

                        v.RuleFor(t => t.settings.LocationScopeId)
                            .Null();

                        v.RuleFor(t => t.settings.LocationScopeType)
                            .Null();

                        v.RuleFor(t => t.settings.DestinationScopeId)
                            .Null();

                        v.RuleFor(t => t.settings.DestinationScopeType)
                            .Null();

                        v.RuleFor(t => t.policy!.DestinationScopeType)
                            .Must(d => d.Equals(DestinationMarkupScopeTypes.Market) || d.Equals(DestinationMarkupScopeTypes.Country) ||
                                d.Equals(DestinationMarkupScopeTypes.Locality))
                            .WithMessage($"Markup policy with Id {policyId} is not destination's location!")
                            .When(t => t.policy is not null && t.policy.SubjectScopeType == SubjectMarkupScopeTypes.NotSpecified);

                        v.RuleFor(t => t.policy!.SubjectScopeType)
                            .Must(d => d.Equals(SubjectMarkupScopeTypes.Market) || d.Equals(SubjectMarkupScopeTypes.Country) ||
                                d.Equals(SubjectMarkupScopeTypes.Locality))
                            .WithMessage($"Markup policy with Id {policyId} is not location!")
                            .When(t => t.policy is not null && (t.policy.DestinationScopeType == DestinationMarkupScopeTypes.NotSpecified ||
                                t.policy.DestinationScopeType == DestinationMarkupScopeTypes.Global));
                    }, (settings, policy));
        }


        public async Task<Result> AddAgencyPolicy(int agencyId, SetAgencyMarkupRequest request)
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency)
                .Where(p => p.SubjectScopeId == agencyId.ToString())
                .SingleOrDefaultAsync();

            var settings = new MarkupPolicySettings("Agency global markup", MarkupFunctionType.Percent,
                request.Percent, Currencies.USD, agencyId.ToString());

            // TODO: Add a validation to not allow negative markup
            // https://github.com/happy-travel/agent-app-project/issues/1244
            if (policy is null)
            {
                var policyData = new MarkupPolicyData(settings,
                    new MarkupPolicyScope(SubjectMarkupScopeTypes.Agency, agencyId));

                return await Add(policyData);
            }

            return await Update(policy.Id, settings);
        }


        public async Task<AgencyMarkupInfo?> GetForAgency(int agencyId)
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agencyId.ToString())
                .SingleOrDefaultAsync();

            return policy is null
                ? null
                : new AgencyMarkupInfo { Percent = policy.Value };
        }


        public async Task<Result> RemoveAgencyPolicy(int agencyId)
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agencyId.ToString())
                .SingleOrDefaultAsync();

            return policy is null
                ? Result.Failure("Could not find agency policy")
                : await Remove(policy.Id);
        }


        public async Task<Result> RemoveLocationPolicy(int policyId)
        {
            var isLocationPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    (p.SubjectScopeType == SubjectMarkupScopeTypes.Market || p.SubjectScopeType == SubjectMarkupScopeTypes.Country ||
                    p.SubjectScopeType == SubjectMarkupScopeTypes.Locality || p.DestinationScopeType == DestinationMarkupScopeTypes.Market ||
                    p.DestinationScopeType == DestinationMarkupScopeTypes.Country || p.DestinationScopeType == DestinationMarkupScopeTypes.Locality)
                    && p.Id == policyId);

            return isLocationPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' was not found or not local");
        }


        // private static Result<MarkupPolicyData> GetPolicyData(MarkupPolicy policy)
        // {
        //     int? agencyId = null, agentId = null;
        //     string? locationId = null;

        //     if (policy.SubjectScopeType == SubjectMarkupScopeTypes.Agency && int.TryParse(policy.SubjectScopeId, out var parsedId))
        //         agencyId = parsedId;

        //     if (policy.SubjectScopeType == SubjectMarkupScopeTypes.Agent)
        //     {
        //         var agentInAgencyId = AgentInAgencyId.Create(policy.SubjectScopeId);
        //         agencyId = agentInAgencyId.AgencyId;
        //         agentId = agentInAgencyId.AgentId;
        //     }

        //     if (policy.SubjectScopeType is SubjectMarkupScopeTypes.Locality or SubjectMarkupScopeTypes.Country or SubjectMarkupScopeTypes.Market)
        //         locationId = policy.SubjectScopeId;

        //     return new MarkupPolicyData(new MarkupPolicySettings(policy.Description, policy.FunctionType, policy.Value, policy.Currency, policy.DestinationScopeId),
        //         new MarkupPolicyScope(policy.SubjectScopeType, agencyId, agentId, locationId));
        // }


        private async Task<Result<SubjectMarkupScopeTypes>> GetAgentMarkupScopeType(MarkupPolicySettings settings)
        {
            if (settings.LocationScopeType == SubjectMarkupScopeTypes.Market)
                return settings.LocationScopeType.Value;

            var (_, isFailure, value, error) = await _mapperClient.GetSlimLocationDescription(settings.LocationScopeId!);
            if (isFailure)
                return Result.Failure<SubjectMarkupScopeTypes>(error.Detail);

            return value.Type switch
            {
                MapperLocationTypes.Country => SubjectMarkupScopeTypes.Country,
                MapperLocationTypes.Locality => SubjectMarkupScopeTypes.Locality,
                _ => Result.Failure<SubjectMarkupScopeTypes>($"Type {value.Type} is not suitable")
            };
        }


        private async Task<Result<DestinationMarkupScopeTypes>> GetDestinationScopeType(string? destinationScopeId)
        {
            // If destinationScopeId is not provided, treat it as Global
            if (string.IsNullOrWhiteSpace(destinationScopeId))
                return DestinationMarkupScopeTypes.Global;

            var (_, isFailure, value, error) = await _mapperClient.GetSlimLocationDescription(destinationScopeId);
            if (isFailure)
                return Result.Failure<DestinationMarkupScopeTypes>(error.Detail);

            return value.Type switch
            {
                MapperLocationTypes.Country => DestinationMarkupScopeTypes.Country,
                MapperLocationTypes.Locality => DestinationMarkupScopeTypes.Locality,
                MapperLocationTypes.Accommodation => DestinationMarkupScopeTypes.Accommodation,
                _ => Result.Failure<DestinationMarkupScopeTypes>($"Type {value.Type} is not suitable")
            };
        }


        private Task<Result> UpdateDisplayedMarkupFormula(MarkupPolicy policy)
        {
            return policy.SubjectScopeType switch
            {
                SubjectMarkupScopeTypes.Agent
                    => _displayedMarkupFormulaService.UpdateAgentFormula(AgentInAgencyId.Create(policy.SubjectScopeId).AgentId, AgentInAgencyId.Create(policy.SubjectScopeId).AgencyId),

                SubjectMarkupScopeTypes.Agency
                    => _displayedMarkupFormulaService.UpdateAgencyFormula(int.Parse(policy.SubjectScopeId)),

                SubjectMarkupScopeTypes.Global
                    => _displayedMarkupFormulaService.UpdateGlobalFormula(),

                _ => Task.FromResult(Result.Success())
            };
        }


        private async Task WriteAuditLog(MarkupPolicy policy, MarkupPolicyEventOperationType type)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();

            var writeLogTask = (AgentScopeType: policy.SubjectScopeType, type) switch
            {
                (SubjectMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Created) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupCreated),
                (SubjectMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Modified) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupUpdated),
                (SubjectMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Deleted) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupDeleted),
                (SubjectMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Created) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupCreated),
                (SubjectMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Modified) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupUpdated),
                (SubjectMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Deleted) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupDeleted),
                (SubjectMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Created) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupCreated),
                (SubjectMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Modified) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupUpdated),
                (SubjectMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Deleted) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupDeleted),
                _ => Task.CompletedTask
            };

            await writeLogTask;


            Task WriteAgentLog(MarkupPolicyEventType eventType)
            {
                var agentInAgencyId = AgentInAgencyId.Create(policy.SubjectScopeId);
                return _markupPolicyAuditService.Write(eventType, new AgentMarkupPolicyData(policy.Id, agentInAgencyId.AgentId, agentInAgencyId.AgencyId), administrator.ToApiCaller());
            }


            Task WriteAgencyLog(MarkupPolicyEventType eventType)
                => _markupPolicyAuditService.Write(eventType, new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.SubjectScopeId)), administrator.ToApiCaller());


            Task WriteGlobalLog(MarkupPolicyEventType eventType)
                => _markupPolicyAuditService.Write(eventType, new GlobalMarkupPolicyData(policy.Id), administrator.ToApiCaller());
        }


        private async Task<Result> Add(MarkupPolicyData policyData)
        {
            var destinationScopeTypeValue = policyData.Settings.DestinationScopeType;

            if (policyData.Settings.DestinationScopeType is null)
            {
                var destinationScopeType = await GetDestinationScopeType(policyData.Settings.DestinationScopeId);
                if (destinationScopeType.IsFailure)
                    return Result.Failure(destinationScopeType.Error);

                destinationScopeTypeValue = destinationScopeType.Value;
            }

            var (_, isFailure, markupPolicy, error) = await Result.Success()
                .Map(SavePolicy)
                .Tap(p => WriteAuditLog(p, MarkupPolicyEventOperationType.Created));

            if (isFailure)
                return Result.Failure(error);

            return await UpdateDisplayedMarkupFormula(markupPolicy);

            async Task<MarkupPolicy> SavePolicy()
            {
                var now = _dateTimeProvider.UtcNow();
                var (type, agencyId, agentId, agentScopeId) = policyData.Scope;
                var settings = policyData.Settings;

                var policy = new MarkupPolicy
                {
                    Description = settings.Description,
                    Currency = settings.Currency,
                    Created = now,
                    Modified = now,
                    SubjectScopeType = type,
                    SubjectScopeId = agentScopeId,
                    DestinationScopeId = settings.DestinationScopeId,
                    DestinationScopeType = destinationScopeTypeValue!.Value,
                    FunctionType = MarkupFunctionType.Percent,
                    Value = settings.Value
                };

                _context.MarkupPolicies.Add(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }


        private async Task<Result> Remove(int policyId)
        {
            var (_, isFailure, markupPolicy, error) = await GetPolicy()
                .Map(DeletePolicy)
                .Tap(p => WriteAuditLog(p, MarkupPolicyEventOperationType.Deleted));

            if (isFailure)
                return Result.Failure(error);

            return await UpdateDisplayedMarkupFormula(markupPolicy);


            async Task<Result<MarkupPolicy>> GetPolicy()
            {
                var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
                return policy == null
                    ? Result.Failure<MarkupPolicy>($"Policy '{policyId}' was not found or not local")
                    : Result.Success(policy);
            }


            async Task<MarkupPolicy> DeletePolicy(MarkupPolicy policy)
            {
                _context.Remove(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }

        private async Task<Result> Update(int policyId, MarkupPolicySettings settings)
        {
            var destinationScopeType = await GetDestinationScopeType(settings.DestinationScopeId);
            if (destinationScopeType.IsFailure)
                return Result.Failure(destinationScopeType.Error);

            var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
                return Result.Failure($"Policy '{policyId}' was not found or not local");

            var (_, isFailure, markupPolicy, error) = await UpdatePolicy()
                .Tap(p => WriteAuditLog(p, MarkupPolicyEventOperationType.Modified));

            if (isFailure)
                return Result.Failure(error);

            return await UpdateDisplayedMarkupFormula(markupPolicy);

            async Task<Result<MarkupPolicy>> UpdatePolicy()
            {
                policy.Description = settings.Description;
                policy.FunctionType = MarkupFunctionType.Percent;
                policy.Value = settings.Value;
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();
                // policy.SubjectScopeId = settings.LocationScopeId;
                // No SubjectScopeType here because changing its type is not allowed
                // policy.DestinationScopeId = settings.DestinationScopeId;
                // policy.DestinationScopeType = destinationScopeType.Value;

                // var policyData = GetPolicyData(policy);
                // if (policyData.IsFailure)
                //     return Result.Failure<MarkupPolicy>(policyData.Error);

                _context.Update(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }


        private readonly IMarkupPolicyTemplateService _templateService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDisplayedMarkupFormulaService _displayedMarkupFormulaService;
        private readonly IAdministratorContext _administratorContext;
        private readonly IMarkupPolicyAuditService _markupPolicyAuditService;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}