using System;
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
            RuleFor(r => r.Nationality).NotEmpty().Length(2);
            RuleFor(r => r.Residency).NotEmpty().Length(2);
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
    }
}