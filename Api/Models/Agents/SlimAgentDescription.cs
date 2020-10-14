namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct SlimAgentDescription
    {
        public SlimAgentDescription(string name, string lastName, string position)
        {
            Name = name;
            LastName = lastName;
            Position = position;
        }
        
        public string Name { get; }
        public string LastName { get; }
        public string Position { get; }
    }
}