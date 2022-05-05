using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Markups.Supplier;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Markups.Validators
{
    public static class SupplierMarkupValidators
    {
        public static Result ValidateAdd(SupplierMarkupRequest request, EdoContext context)
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


                        Func<string, CancellationToken, Task<bool>> DestinationMarkupDoesNotExist()
                            => async (scopeId, cancelationToken)
                                => !(await context.MarkupPolicies
                                    .AnyAsync(m => m.DestinationScopeId == scopeId && m.SupplierCode == request.SupplierCode, cancelationToken));


                        Func<string, CancellationToken, Task<bool>> CountryExists()
                            => async (scopeId, cancelationToken)
                                => await context.Countries.AnyAsync(m => m.Code == scopeId, cancelationToken);
                    }, request);


        public static Result ValidateModify((SupplierMarkupRequest request, MarkupPolicy? policy) entity)
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
                        .WithMessage($"Modifying markup policy was not found!");

                    if (entity.policy is not null)
                    {
                        v.RuleFor(t => t.request.DestinationScopeId)
                            .Must(d => d.Equals(entity.policy!.DestinationScopeId))
                            .WithMessage($"Modifying DestinationScopeId is prohibited!");

                        v.RuleFor(t => t.request.DestinationScopeType)
                            .Must(d => d.Equals(entity.policy!.DestinationScopeType))
                            .WithMessage($"Modifying DestinationScopeType is prohibited!");

                        v.RuleFor(t => t.request.SupplierCode)
                            .Must(d => d.Equals(entity.policy!.SupplierCode))
                            .WithMessage($"Modifying SupplierCode is prohibited!");

                        var message = $"Markup policy with Id {entity.policy!.Id} is not supplier country markup!";

                        v.RuleFor(t => t.policy!.DestinationScopeType)
                            .Must(d => d.Equals(DestinationMarkupScopeTypes.Country))
                            .WithMessage(message);

                        v.RuleFor(t => t.policy!.SubjectScopeType)
                            .Must(d => d.Equals(SubjectMarkupScopeTypes.Global))
                            .WithMessage(message);

                        v.RuleFor(t => t.policy!.SupplierCode)
                            .NotNull().WithMessage(message)
                            .NotEmpty().WithMessage(message);
                    }
                }, (entity.request, entity.policy));
    }
}