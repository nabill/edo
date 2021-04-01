using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public static class AgencyValidator
    {
        public static Result Validate(in AgencyInfo agencyInfo)
        {
            return GenericValidator<AgencyInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Name).NotEmpty();
                v.RuleFor(c => c.Address).NotEmpty();
                v.RuleFor(c => c.City).NotEmpty();
                v.RuleFor(c => c.Phone).NotEmpty().Matches(@"^[0-9]{3,30}$");
                v.RuleFor(c => c.Fax).Matches(@"^[0-9]{3,30}$").When(i => !string.IsNullOrWhiteSpace(i.Fax));
            }, agencyInfo);
        }


        public static Result Validate(in EditAgencyInfo agencyInfo)
        {
            // Here we allow Address and Phone to be null, which indicates that values are not to be changed.
            return GenericValidator<EditAgencyInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Address).NotEmpty().When(i => i.Address != null);
                v.RuleFor(c => c.Phone).Matches(@"^[0-9]{3,30}$").When(i => i.Phone != null);
                v.RuleFor(c => c.Fax).Matches(@"^[0-9]{3,30}$").When(i => !string.IsNullOrWhiteSpace(i.Fax));
            }, agencyInfo);
        }
    }
}