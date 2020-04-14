using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class InCounterpartyPermissionsExtensions
    {
        public static List<InCounterpartyPermissions> ToList(this InCounterpartyPermissions permissions)
        {
            return InCounterpartyPermissionValues
                .Where(v => permissions.HasFlag(v))
                .ToList();
        }


        private static readonly List<InCounterpartyPermissions> InCounterpartyPermissionValues = Enum.GetValues(typeof(InCounterpartyPermissions))
            .Cast<InCounterpartyPermissions>()
            .ToList();
    }
}