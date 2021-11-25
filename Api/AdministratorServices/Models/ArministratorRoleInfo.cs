using HappyTravel.Edo.Common.Enums.Administrators;
using System.Collections.Generic;
using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public readonly struct AdministratorRoleInfo
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public List<AdministratorPermissions> Permissions { get; init; }

        public NotificationTypes[] NotificationTypes { get; init; }
    }
}
