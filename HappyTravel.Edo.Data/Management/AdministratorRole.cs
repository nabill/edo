﻿using HappyTravel.Edo.Common.Enums.Administrators;

namespace HappyTravel.Edo.Data.Management
{
    public class AdministratorRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AdministratorPermissions Permissions { get; set; }
    }
}
