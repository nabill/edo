namespace HappyTravel.Edo.Api.Models.Agents
{
    // TODO: Replace with more appropriate model during NIJO-820
    public readonly struct AgentInfoInAgency
    {
        public AgentInfoInAgency(int agentId, string firstName, string lastName, string email,
            string title, string position, int agencyId, string agencyName,
            bool isMaster, int[] inAgencyRoleIds, bool isActive)
        {
            AgentId = agentId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Title = title;
            Position = position;
            AgencyId = agencyId;
            AgencyName = agencyName;
            IsMaster = isMaster;
            InAgencyRoleIds = inAgencyRoleIds;
            IsActive = isActive;
        }


        /// <summary>
        ///     Agent ID.
        /// </summary>
        public int AgentId { get; }

        /// <summary>
        ///     First name.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        ///     Last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        ///     Agent e-mail.
        /// </summary>
        public string Email { get; }

        /// <summary>
        ///     ID of the agent's agency.
        /// </summary>
        public int AgencyId { get; }

        /// <summary>
        ///     Name of the agent's agency.
        /// </summary>
        public string AgencyName { get; }

        /// <summary>
        ///     Indicates whether the agent is master or regular agent.
        /// </summary>
        public bool IsMaster { get; }

        /// <summary>
        ///     Title (Mr., Mrs etc).
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Agent position in agency.
        /// </summary>
        public string Position { get; }

        /// <summary>
        ///     Roles of the agent.
        /// </summary>
        public int[] InAgencyRoleIds { get; }

        /// <summary>
        /// Activity state
        /// </summary>
        public bool IsActive { get; }
    }
}