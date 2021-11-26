using System.Linq;
using FluentValidation;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class AccommodationBookingRequestValidator : AbstractValidator<AccommodationBookingRequest>
    {
        public AccommodationBookingRequestValidator()
        {
            RuleFor(r => r.AccommodationId).NotEmpty();
            RuleFor(r => r.SearchId).NotNull();
            RuleFor(r => r.RoomContractSetId).NotNull();
            RuleFor(r => r.ReferenceCode).NotEmpty();
            RuleFor(r => r.Nationality).NotEmpty().MaximumLength(2).MinimumLength(2).Must(HasOnlyLetters);
            RuleFor(r => r.Residency).NotEmpty().MaximumLength(2).MinimumLength(2).Must(HasOnlyLetters);
            RuleFor(r => r.RoomDetails).NotEmpty();
            RuleForEach(r => r.RoomDetails).SetValidator(new RoomDetailsValidator());
        }
        
        
        private static bool HasOnlyLetters(string str) 
            => str.All(char.IsLetter);
    }
}