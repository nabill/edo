namespace HappyTravel.Edo.Api.Models.Agents
{
    public record MasterAgentContext
    {
        public int AgentId { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public int AgencyId { get; init; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
