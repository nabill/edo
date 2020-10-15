namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct SlimAgentDescription
    {
        public SlimAgentDescription(int id, string firstName, string lastName, string position)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
        }

        /// <summary>
        /// Agent id
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Agent's First Name
        /// </summary>
        public string FirstName { get; }
        
        // Agent's Last Name
        public string LastName { get; }
        
        /// <summary>
        /// Agent's position
        /// </summary>
        public string Position { get; }
    }
}