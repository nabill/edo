namespace HappyTravel.Edo.Api.Models.Management.Administrators
{
    public readonly struct AdministratorInfo
    {
        public AdministratorInfo(int id, string firstName, string lastName, string position, int[] administratorRoleIds,
            bool isActive)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            AdministratorRoleIds = administratorRoleIds;
            IsActive = isActive;
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
        /// Flag of administrator activity state
        /// </summary>
        public bool IsActive { get; }
    }
}