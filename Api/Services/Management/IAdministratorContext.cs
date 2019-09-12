using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IAdministratorContext
    {
        Task<bool> HasGlobalPermission(GlobalPermissions permission);
    }
}