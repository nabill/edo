using FluentValidation;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class RoomOccupationRequestValidator : AbstractValidator<RoomOccupationRequest>
    {
        public RoomOccupationRequestValidator()
        {
            RuleFor(r => r.AdultsNumber).GreaterThan(0);
            RuleForEach(r => r.ChildrenAges).GreaterThanOrEqualTo(0).LessThan(100);
            RuleFor(r => r.IsExtraBedNeeded).NotNull();
        }
    }
}