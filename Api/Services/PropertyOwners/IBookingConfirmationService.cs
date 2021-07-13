using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.PropertyOwners;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.PropertyOwners
{
    public interface IBookingConfirmationService
    {
        Task<Result> Update(string referenceCode, BookingConfirmation bookingConfirmation);
        Task<Result<SlimBookingConfirmation>> Get(string referenceCode);
    }
}