using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Data.Management;
using System.Linq;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AdministratorRoleExtensions
    {
        public static AdministratorRoleInfo ToAdministratorRoleInfo(this AdministratorRole administratorRole)
            => new()
            {
                Id = administratorRole.Id,
                Name = administratorRole.Name,
                Permissions = administratorRole.Permissions.ToList()
            };


        public static AdministratorRole ToAdministratorRole(this AdministratorRoleInfo administratorRoleInfo)
            => new()
            {
                Name = administratorRoleInfo.Name,
                Permissions = administratorRoleInfo.Permissions.Any()
                    ? administratorRoleInfo.Permissions.Aggregate((p1, p2) => p1 | p2)
                    : default
            };
    }
}