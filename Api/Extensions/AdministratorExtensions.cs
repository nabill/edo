﻿using HappyTravel.Edo.Api.Models.Management.Administrators;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AdministratorExtensions
    {
        public static AdministratorInfo ToAdministratorInfo(this Administrator administrator)
            => new AdministratorInfo(
                id: administrator.Id,
                firstName: administrator.FirstName,
                lastName: administrator.LastName,
                position: administrator.Position,
                administratorRoleIds: administrator.AdministratorRoleIds,
                isActive: administrator.IsActive);
    }
}
