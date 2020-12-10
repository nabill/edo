using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IAppliedBookingMarkupRecordsManager
    {
        Task Create(Booking booking, IEnumerable<AppliedMarkup> appliedMarkups);
    }
}