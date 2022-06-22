using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IBookingRecordManager
    {
        Task Update(Booking booking);

        Task<Result<Booking>> Get(string referenceCode);

        Task<Result<Booking>> Get(int id);
    }
}