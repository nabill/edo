using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Search;
using Microsoft.EntityFrameworkCore;

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
            RuleFor(r => r.Nationality).NotEmpty().MaximumLength(2).MinimumLength(2).MustAsync(IsCountryIsoCode).WithMessage("Wrong country ISO code");
            RuleFor(r => r.Residency).NotEmpty().MaximumLength(2).MinimumLength(2).MustAsync(IsCountryIsoCode).WithMessage("Wrong country ISO code");
        }


        private static bool IsCheckInDateGreaterThanCheckOutDate(AvailabilityRequest request) 
            => (request.CheckOutDate - request.CheckInDate).TotalDays > 0;


        private Task<bool> IsCountryIsoCode(string code, CancellationToken cancellationToken) 
            => _context.Countries.AnyAsync(c => c.Code == code, cancellationToken);


        private readonly EdoContext _context;
    }
}