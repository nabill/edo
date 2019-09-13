using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IAdministratorContext
    {
        Task<bool> HasGlobalPermission(GlobalPermissions permission);
        Task<Result<Administrator>> GetCurrent();
    }
}