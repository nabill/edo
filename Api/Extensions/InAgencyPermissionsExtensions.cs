using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class InAgencyPermissionsExtensions
    {
        public static List<InAgencyPermissions> ToList(this InAgencyPermissions permissions)
        {
            return InAgencyPermissionValues
                .Where(v => permissions.HasFlag(v))
                .ToList();
        }


        public static InAgencyPermissions ToFlags(this List<InAgencyPermissions> permissions)
        {
            return permissions.Any()
                ? permissions.Aggregate((p1, p2) => p1 | p2)
                : default;
        }


        private static readonly List<InAgencyPermissions> InAgencyPermissionValues = Enum.GetValues(typeof(InAgencyPermissions))
            .Cast<InAgencyPermissions>()
            .ToList();
    }
}