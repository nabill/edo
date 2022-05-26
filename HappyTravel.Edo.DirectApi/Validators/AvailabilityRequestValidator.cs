using System;
using System.Linq;
using FluentValidation;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Validators
{
    public class AvailabilityRequestValidator : AbstractValidator<AvailabilityRequest>
    {
        public AvailabilityRequestValidator(EdoContext context)
        {
            _context = context;
            
            
            RuleFor(r => r.Ids).NotEmpty();
            RuleForEach(r => r.Ids).NotEmpty();
            RuleFor(r => r.CheckInDate).NotEmpty().GreaterThan(DateTime.Now.Date);
            RuleFor(r => r.CheckOutDate).NotEmpty().Must((request, _) => IsCheckInDateGreaterThanCheckOutDate(request));
            RuleFor(r => r.RoomDetails).NotEmpty();
            RuleForEach(r => r.RoomDetails).SetValidator(new RoomOccupationRequestValidator());
            RuleFor(r => r.Nationality).NotEmpty().MaximumLength(2).MinimumLength(2).Must(IsCountryIsoCode).WithMessage("Wrong country ISO code");
            RuleFor(r => r.Residency).NotEmpty().MaximumLength(2).MinimumLength(2).Must(IsCountryIsoCode).WithMessage("Wrong country ISO code");
        }


        private static bool IsCheckInDateGreaterThanCheckOutDate(AvailabilityRequest request) 
            => (request.CheckOutDate - request.CheckInDate).TotalDays > 0;


        private bool IsCountryIsoCode(string code) 
            => _context.Countries.Any(c => c.Code == code);


        private readonly EdoContext _context;
    }
}