namespace HappyTravel.Edo.Api.Models.Agents
{
    public struct SlimAgentDescription
    {
        public SlimAgentDescription(int id, string firstName, string lastName, string position)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
        }

        // TODO: replace to readonly struct with init properties after upgrade to C# 9

        /// <summary>
        /// Agent id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Agent's First Name
        /// </summary>
        public string FirstName { get; set; }
        
        // Agent's Last Name
        public string LastName { get; set; }
        
        /// <summary>
        /// Agent's position
        /// </summary>
        public string Position { get; set; }
    }
}