using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using HappyTravel.Edo.DirectApi.Models.Booking;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class RoomDetailsValidator : AbstractValidator<BookingRoomDetails>
    {
        public RoomDetailsValidator()
        {
            RuleFor(d => d.Passengers).NotEmpty();
            RuleForEach(d => d.Passengers).SetValidator(new PaxValidator());
        }
    }
}