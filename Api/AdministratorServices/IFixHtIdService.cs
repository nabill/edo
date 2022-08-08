using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IFixHtIdService
    {
        Task FillEmptyHtIds();
        Task<List<int>> FixAccommodationIds(BookingCreationPeriod request, CancellationToken cancellationToken = default);
    }
}