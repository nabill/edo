using FluentValidation;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class RoomOccupationRequestValidator : AbstractValidator<RoomOccupationRequest>
    {
        public RoomOccupationRequestValidator()
        {
            RuleFor(r => r.AdultsNumber).GreaterThan(0);
            RuleForEach(r => r.ChildrenAges).GreaterThanOrEqualTo(0).LessThan(100);
        }
    }
}