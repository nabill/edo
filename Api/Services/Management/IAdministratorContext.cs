using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IAdministratorContext
    {
        Task<bool> HasPermission(AdministratorPermissions permission);

        Task<Result<Administrator>> GetCurrent();
    }
}