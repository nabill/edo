using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public static class AgencyValidator
    {
        public static Result Validate(in RegistrationAgencyInfo agencyInfo)
        {
            return GenericValidator<RegistrationAgencyInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Name).NotEmpty();
                v.RuleFor(c => c.Address).NotEmpty();
                v.RuleFor(c => c.City).NotEmpty();
                v.RuleFor(c => c.Phone).NotEmpty();
                v.RuleFor(c => c.BillingEmail).EmailAddress().When(i => !string.IsNullOrWhiteSpace(i.BillingEmail));
            }, agencyInfo);
        }
    }
}