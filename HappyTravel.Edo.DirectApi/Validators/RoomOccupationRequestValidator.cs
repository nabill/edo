using FluentValidation;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class RoomOccupationRequestValidator : AbstractValidator<RoomOccupationRequest>
    {
        public RoomOccupationRequestValidator()
        {
            RuleFor(r => r.AdultsNumber).GreaterThan(0);
        }
    }
}