using HappyTravel.Edo.Common.Enums.Administrators;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IExternalAdminContext
    {
        bool HasPermission(AdministratorPermissions permission);
    }
}