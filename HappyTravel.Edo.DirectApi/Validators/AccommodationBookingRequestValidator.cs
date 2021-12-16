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
            RuleFor(r => r.RoomDetails).NotEmpty();
            RuleForEach(r => r.RoomDetails).SetValidator(new RoomDetailsValidator());
            RuleFor(r => r.Nationality).NotEmpty().MaximumLength(2).MinimumLength(2).MustAsync(IsCountryIsoCode).WithMessage("Wrong country ISO code");
            RuleFor(r => r.Residency).NotEmpty().MaximumLength(2).MinimumLength(2).MustAsync(IsCountryIsoCode).WithMessage("Wrong country ISO code");
        }
        
        
        private Task<bool> IsCountryIsoCode(string code, CancellationToken cancellationToken) 
            => _context.Countries.AnyAsync(c => c.Code == code, cancellationToken);


        private readonly EdoContext _context;
    }
}