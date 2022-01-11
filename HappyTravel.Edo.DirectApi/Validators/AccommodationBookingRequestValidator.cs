using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Models.Booking;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class AccommodationBookingRequestValidator : AbstractValidator<AccommodationBookingRequest>
    {
        public AccommodationBookingRequestValidator(EdoContext context)
        {
            _context = context;
            
            
            RuleFor(r => r.AccommodationId).NotEmpty();
            RuleFor(r => r.SearchId).NotNull();
            RuleFor(r => r.RoomContractSetId).NotNull();
            RuleFor(r => r.ClientReferenceCode).NotEmpty();
            RuleFor(r => r.RoomDetails).NotEmpty().Must(HasLeader).WithMessage("Passengers doesn't have a leader");
            RuleForEach(r => r.RoomDetails).SetValidator(new RoomDetailsValidator());
        }
        
        
        private static bool HasLeader(List<BookingRoomDetails> rooms) 
            => rooms.Any(r => r.Passengers.Any(p => p.IsLeader));


        private readonly EdoContext _context;
    }
}