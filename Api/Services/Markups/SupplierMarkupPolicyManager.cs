using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Markups.Supplier;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Messaging;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Markups
{
    public class SupplierMarkupPolicyManager : ISupplierMarkupPolicyManager
    {
        public SupplierMarkupPolicyManager(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IDisplayedMarkupFormulaService displayedMarkupFormulaService,
            IMessageBus messageBus)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _displayedMarkupFormulaService = displayedMarkupFormulaService;
            _messageBus = messageBus;
        }


        public Task<List<MarkupInfo>> Get(CancellationToken cancellationToken)
            => _context.MarkupPolicies
                .Where(p => !string.IsNullOrEmpty(p.SupplierCode))
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync(cancellationToken);


        public async Task<Result> Add(SupplierMarkupRequest request, CancellationToken cancellationToken)
        {
            return await ValidateAdd(request)
                .Tap(AddPolicy);


            Result ValidateAdd(SupplierMarkupRequest request)
                => GenericValidator<SupplierMarkupRequest>.Validate(v =>
                    {
                        var valueValidatorMessage = "Markup policy value must be in range (-100..-0.1) or (0.1..100)";

                        v.RuleFor(r => Math.Abs(r.Value))
                            .GreaterThanOrEqualTo(0.1m)
                            .WithMessage(valueValidatorMessage)
                            .LessThanOrEqualTo(100m)
                            .WithMessage(valueValidatorMessage);

                        v.RuleFor(r => r.SupplierCode)
                            .NotNull()
                            .NotEmpty();

                        v.RuleFor(r => r.DestinationScopeId)
                            .NotNull()
                            .MustAsync(DestinationMarkupDoesNotExist()!)
                            .When(r => r.DestinationScopeType == DestinationMarkupScopeTypes.Country, ApplyConditionTo.CurrentValidator)
                            .WithMessage(r => $"Destination markup policy with DestinationScopeId {r.DestinationScopeId} already exists or unexpected value!")
                            .MustAsync(CountryExists()!)
                            .When(m => m.DestinationScopeType == DestinationMarkupScopeTypes.Country, ApplyConditionTo.CurrentValidator)
                            .WithMessage(m => $"Country with code {m.DestinationScopeId} doesn't exist!");

                        v.RuleFor(m => m.DestinationScopeType)
                            .NotNull()
                            .Must(d => d.Equals(DestinationMarkupScopeTypes.Country))
                            .WithMessage($"Request's destinationScopeType must be Country");
                    }, request);


            Func<string, CancellationToken, Task<bool>> DestinationMarkupDoesNotExist()
                => async (scopeId, cancelationToken)
                    => !(await _context.MarkupPolicies
                        .AnyAsync(m => m.DestinationScopeId == scopeId && m.SupplierCode == request.SupplierCode, cancelationToken));


            Func<string, CancellationToken, Task<bool>> CountryExists()
                => async (scopeId, cancelationToken)
                    => await _context.Countries.AnyAsync(m => m.Code == scopeId, cancelationToken);


            async Task<Result> AddPolicy()
            {
                return await Result.Success()
                    .Map(SavePolicy)
                    .Bind(UpdateDisplayedMarkupFormula)
                    .Tap(() => _messageBus.Publish(MessageBusTopics.MarkupPolicyUpdated));

                async Task<MarkupPolicy> SavePolicy()
                {
                    var now = _dateTimeProvider.UtcNow();

                    var policy = new MarkupPolicy
                    {
                        Description = request.Description,
                        Currency = Currencies.USD,
                        Created = now,
                        Modified = now,
                        SubjectScopeType = SubjectMarkupScopeTypes.Global,
                        SubjectScopeId = string.Empty,
                        DestinationScopeId = request.DestinationScopeId,
                        DestinationScopeType = request.DestinationScopeType,
                        FunctionType = MarkupFunctionType.Percent,
                        Value = request.Value
                    };

                    _context.MarkupPolicies.Add(policy);
                    await _context.SaveChangesAsync(cancellationToken);

                    return policy;
                }
            }
        }


        public async Task<Result> Modify(int policyId, SupplierMarkupRequest request, CancellationToken cancellationToken)
        {
            var policy = await _context.MarkupPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

            return await ValidateModify((request, policy))
                .Tap(ModifyPolicy);


            async Task<Result> ModifyPolicy()
                => await Modify(policyId, request);


            Result ValidateModify((SupplierMarkupRequest request, MarkupPolicy? policy) entity)
                => GenericValidator<(SupplierMarkupRequest request, MarkupPolicy? policy)>.Validate(v =>
                    {
                        var valueValidatorMessage = "Markup policy value must be in range (-100..-0.1) or (0.1..100)";

                        v.RuleFor(t => Math.Abs(t.request.Value))
                            .GreaterThanOrEqualTo(0.1m)
                            .WithMessage(valueValidatorMessage)
                            .LessThanOrEqualTo(100m)
                            .WithMessage(valueValidatorMessage);

                        v.RuleFor(t => t.policy)
                            .NotNull()
                            .WithMessage($"Markup policy with Id {policyId} was not found!");

                        v.RuleFor(t => t.request.DestinationScopeId)
                            .Must(d => d.Equals(policy!.DestinationScopeId))
                            .WithMessage($"Modifying DestinationScopeId is prohibited!")
                            .When(t => t.policy is not null);

                        v.RuleFor(t => t.request.DestinationScopeType)
                            .Must(d => d.Equals(policy!.DestinationScopeType))
                            .WithMessage($"Modifying DestinationScopeType is prohibited!")
                            .When(t => t.policy is not null);

                        v.RuleFor(t => t.request.SupplierCode)
                            .Must(d => d.Equals(policy!.SupplierCode))
                            .WithMessage($"Modifying SupplierCode is prohibited!")
                            .When(t => t.policy is not null);

                        v.RuleFor(t => t.policy!.DestinationScopeType)
                            .Must(d => d.Equals(DestinationMarkupScopeTypes.Country))
                            .WithMessage($"Markup policy with Id {policyId} is not supplier country markup!")
                            .When(t => t.policy is not null);

                        v.RuleFor(t => t.policy!.SubjectScopeType)
                            .Must(d => d.Equals(SubjectMarkupScopeTypes.Global))
                            .WithMessage($"Markup policy with Id {policyId} is not supplier country markup!")
                            .When(t => t.policy is not null);

                        v.RuleFor(t => t.policy!.SupplierCode)
                            .NotNull()
                            .NotEmpty()
                            .WithMessage($"Markup policy with Id {policyId} is not supplier country markup!")
                            .When(t => t.policy is not null);
                    }, (request, policy));


            async Task<Result> Modify(int policyId, SupplierMarkupRequest request)
            {
                var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId, cancellationToken);
                if (policy == null)
                    return Result.Failure($"Policy '{policyId}' was not found or not local");

                return await UpdatePolicy()
                    .Bind(UpdateDisplayedMarkupFormula)
                    .Tap(() => _messageBus.Publish(MessageBusTopics.MarkupPolicyUpdated));

                async Task<Result<MarkupPolicy>> UpdatePolicy()
                {
                    policy.Description = request.Description;
                    policy.Value = request.Value;
                    policy.Modified = _dateTimeProvider.UtcNow();

                    _context.Update(policy);
                    await _context.SaveChangesAsync(cancellationToken);

                    return policy;
                }
            }
        }


        public async Task<Result> Remove(int policyId, CancellationToken cancellationToken)
        {
            var isSupplierPolicy = await _context.MarkupPolicies
                .AnyAsync(p => !string.IsNullOrEmpty(p.SupplierCode) && p.Id == policyId, cancellationToken);

            return isSupplierPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' was not found or not local");


            async Task<Result> Remove(int policyId)
            {
                return await GetPolicy()
                    .Map(DeletePolicy)
                    .Bind(UpdateDisplayedMarkupFormula)
                    .Tap(() => _messageBus.Publish(MessageBusTopics.MarkupPolicyUpdated));


                async Task<Result<MarkupPolicy>> GetPolicy()
                {
                    var policy = await _context.MarkupPolicies
                        .SingleOrDefaultAsync(p => p.Id == policyId, cancellationToken);
                    return policy == null
                        ? Result.Failure<MarkupPolicy>($"Policy '{policyId}' was not found or not local")
                        : Result.Success(policy);
                }


                async Task<MarkupPolicy> DeletePolicy(MarkupPolicy policy)
                {
                    _context.Remove(policy);
                    await _context.SaveChangesAsync(cancellationToken);

                    return policy;
                }
            }
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


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDisplayedMarkupFormulaService _displayedMarkupFormulaService;
        private readonly IMessageBus _messageBus;
    }
}