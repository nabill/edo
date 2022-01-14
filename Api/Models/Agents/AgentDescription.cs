using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentDescription
    {
        public AgentDescription(int id, string email, string lastName, string firstName, string title, string position,
            List<AgentAgencyRelationInfo> agencyRelations)
        {
            Id = id;
            Email = email;
            LastName = lastName;
            FirstName = firstName;
            Title = title;
            Position = position;
            AgencyRelations = agencyRelations;
        }


        /// <summary>
        ///     Agent Id
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Agent e-mail.
        /// </summary>
        public string Email { get; }

        /// <summary>
        ///     Last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        ///     First name.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        ///     Title (Mr., Mrs etc).
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Agent position in agency.
        /// </summary>
        public string Position { get; }

        /// <summary>
        ///     List of agencies, associated with agent.
        /// </summary>
        public List<AgentAgencyRelationInfo> AgencyRelations { get; }
    }
}