using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class AppliedBookingMarkupRecordsManager : IAppliedBookingMarkupRecordsManager
    {
        public AppliedBookingMarkupRecordsManager(EdoContext context) => _context = context;

        public async Task Create(Booking booking, IEnumerable<AppliedMarkup> appliedMarkups)
        {
            _context.AppliedBookingMarkups.AddRange(appliedMarkups
                .Select(a => new AppliedBookingMarkup
                {
                    ReferenceCode = booking.ReferenceCode,
                    PolicyId = a.PolicyId,
                    Amount = a.AmountChange.Amount,
                    Currency = a.AmountChange.Currency
                }));
            await _context.SaveChangesAsync();
        }


        private readonly EdoContext _context;
    }
}