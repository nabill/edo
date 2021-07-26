using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.Administrators;

namespace HappyTravel.Edo.Api.Models.Management.Administrators
{
    public readonly struct RichAdministratorInfo
    {
        public RichAdministratorInfo(int id, string firstName, string lastName, string position, int[] administratorRoleIds,
            bool isActive, List<AdministratorPermissions> permissions)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            AdministratorRoleIds = administratorRoleIds;
            IsActive = isActive;
            Permissions = permissions;
        }
        
        /// <summary>
        /// Id of administrator
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; }
        
        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; }
        
        /// <summary>
        /// Position
        /// </summary>
        public string Position { get; }

        /// <summary>
        /// Ids of administrator roles
        /// </summary>
        public int[] AdministratorRoleIds { get; }

        /// <summary>
        /// All permissions, granted by the roles
        /// </summary>
        public List<AdministratorPermissions> Permissions { get; }

        /// <summary>
        /// Flag of administrator activity state
        /// </summary>
        public bool IsActive { get; }
    }
}