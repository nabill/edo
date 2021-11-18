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
            RuleForEach(r => r.Ids).Must(IsValidHtId);
            RuleFor(r => r.CheckInDate).GreaterThan(DateTime.Now.Date);
            RuleFor(r => r.CheckOutDate).Must((request, _) => IsCheckInDateGreaterThanCheckOutDate(request));
            RuleFor(r => r.Nationality).Must(IsAlpha2String).WithMessage("Must be alpha-2 country code");
            RuleFor(r => r.Residency).Must(IsAlpha2String).WithMessage("Must be alpha-2 country code");
            RuleFor(r => r.RoomDetails).NotEmpty();
        }


        private static bool IsValidHtId(string htId)
        {
            if (string.IsNullOrWhiteSpace(htId))
                return false;

            return htId.Length is > 5 and <= 30;
        }


        private static bool IsCheckInDateGreaterThanCheckOutDate(AvailabilityRequest request) 
            => (request.CheckOutDate - request.CheckInDate).TotalDays > 0;


        private static bool IsAlpha2String(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            if (str.Length != 2)
                return false;

            return str.All(char.IsLetter);
        }
    }
}