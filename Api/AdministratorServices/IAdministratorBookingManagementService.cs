using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAdministratorBookingManagementService
    {
        Task<Result> Cancel(int bookingId, Administrator admin, bool requireSupplierConfirmation);

        Task<Result> Discard(int bookingId, Administrator admin);
        
        Task<Result> RefreshStatus(int bookingId, Administrator admin);
    }
}