using FluentValidation;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class RoomDetailsValidator : AbstractValidator<BookingRoomDetails>
    {
        public RoomDetailsValidator()
        {
            RuleFor(d => d.Passengers).NotEmpty();
            RuleFor(d => d.IsExtraBedNeeded).NotNull();
            RuleForEach(d => d.Passengers).SetValidator(new PaxValidator());
        }
    }
}