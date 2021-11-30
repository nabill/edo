using FluentValidation;
using HappyTravel.EdoContracts.General;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class PaxValidator : AbstractValidator<Pax>
    {
        public PaxValidator()
        {
            RuleFor(p => p.Age).GreaterThan(0).LessThan(100);
            RuleFor(p => p.Title).IsInEnum();
            RuleFor(p => p.FirstName).NotEmpty();
            RuleFor(p => p.LastName).NotEmpty();
        }
    }
}