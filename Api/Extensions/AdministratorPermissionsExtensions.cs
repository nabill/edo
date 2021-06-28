using HappyTravel.Edo.Common.Enums.Administrators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AdministratorPermissionsExtensions
    {
        public static List<AdministratorPermissions> ToList(this AdministratorPermissions permissions)
        {
            return AdministratorPermissionValues
                .Where(v => permissions.HasFlag(v))
                .ToList();
        }


        public static AdministratorPermissions ToFlags(this List<AdministratorPermissions> permissions)
        {
            return permissions.Any()
                ? permissions.Aggregate((p1, p2) => p1 | p2)
                : default;
        }


        private static readonly List<AdministratorPermissions> AdministratorPermissionValues = Enum.GetValues(typeof(AdministratorPermissions))
            .Cast<AdministratorPermissions>()
            .ToList();
    }
}
