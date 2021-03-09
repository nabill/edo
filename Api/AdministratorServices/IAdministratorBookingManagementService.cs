using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAdministratorBookingManagementService
    {
        Task<Result> Cancel(int bookingId, Administrator admin);

        Task<Result> Discard(int bookingId, Administrator admin);
        
        Task<Result> RefreshStatus(int bookingId, Administrator admin);

        Task<Result> CancelManually(int bookingId, DateTime cancellationDate, string reason, Administrator admin);
        
        Task<Result> RejectManually(int bookingId, string reason, Administrator admin);
    }
}