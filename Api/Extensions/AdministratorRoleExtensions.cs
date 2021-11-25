using System;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AdministratorRoleExtensions
    {
        public static AdministratorRoleInfo ToAdministratorRoleInfo(this AdministratorRole administratorRole)
            => new()
            {
                Id = administratorRole.Id,
                Name = administratorRole.Name,
                Permissions = administratorRole.Permissions.ToList(),
                NotificationTypes = administratorRole.NotificationTypes ?? Array.Empty<NotificationTypes>()
            };


        public static AdministratorRole ToAdministratorRole(this AdministratorRoleInfo administratorRoleInfo)
            => new()
            {
                Name = administratorRoleInfo.Name,
                Permissions = administratorRoleInfo.Permissions.ToFlags(),
                NotificationTypes = administratorRoleInfo.NotificationTypes ?? Array.Empty<NotificationTypes>()
            };
    }
}