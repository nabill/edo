using System;

namespace HappyTravel.Edo.Data.Agents
{
    public class Agent
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IdentityHash { get; set; } = string.Empty;
        public string? AppSettings { get; set; }
        public string? UserSettings { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}