using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public static class CounterpartyValidator
    {
        public static Result Validate(in RegistrationCounterpartyInfo counterpartyInfo)
        {
            return GenericValidator<RegistrationCounterpartyInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Name).NotEmpty();
            }, counterpartyInfo);
        }
    }
}