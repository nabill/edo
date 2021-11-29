using System;
using System.Linq;
using FluentValidation;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class AvailabilityRequestValidator : AbstractValidator<AvailabilityRequest>
    {
        public AvailabilityRequestValidator()
        {
            RuleFor(r => r.Ids).NotEmpty();
            RuleForEach(r => r.Ids).NotEmpty();
            RuleFor(r => r.CheckInDate).NotEmpty().GreaterThan(DateTime.Now.Date);
            RuleFor(r => r.CheckOutDate).NotEmpty().Must((request, _) => IsCheckInDateGreaterThanCheckOutDate(request));
            RuleFor(r => r.Nationality).NotEmpty().MaximumLength(2).MinimumLength(2).Must(HasOnlyLetters);
            RuleFor(r => r.Residency).NotEmpty().MaximumLength(2).MinimumLength(2).Must(HasOnlyLetters);
            RuleFor(r => r.RoomDetails).NotEmpty();
            RuleForEach(r => r.RoomDetails).SetValidator(new RoomOccupationRequestValidator());
        }


        private static bool IsCheckInDateGreaterThanCheckOutDate(AvailabilityRequest request) 
            => (request.CheckOutDate - request.CheckInDate).TotalDays > 0;


        private static bool HasOnlyLetters(string str) 
            => str.All(char.IsLetter);
    }
}