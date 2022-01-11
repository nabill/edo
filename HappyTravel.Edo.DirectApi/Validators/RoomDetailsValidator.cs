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
            RuleFor(d => d.Passengers).NotEmpty().Must(HasLeader).WithMessage("Passengers doesn't have a leader");
            RuleForEach(d => d.Passengers).SetValidator(new PaxValidator());
        }


        private bool HasLeader(List<Pax> passengers) 
            => passengers.Any(p => p.IsLeader);
    }
}