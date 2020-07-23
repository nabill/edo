using HappyTravel.Edo.Api.Models.Management.Enums;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IExternalAdminContext
    {
        bool HasPermission(AdministratorPermissions permission);
    }
}