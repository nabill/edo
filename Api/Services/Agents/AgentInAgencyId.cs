using System;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentInAgencyId
    {
        public int AgentId { get; }
        public int AgencyId { get; }


        private AgentInAgencyId(int agentId, int agencyId)
        {
            AgentId = agentId;
            AgencyId = agencyId;
        }

        
        public override string ToString() 
            => $"{AgentId}{Delimiter}{AgencyId}";


        public static AgentInAgencyId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Could not get agent in agency id from empty string");

            var parts = value.Split(Delimiter);
            if (parts.Length != 2)
                throw new ArgumentException($"Could not parse '{value}'", nameof(value));

            if (!int.TryParse(parts[0], out var agentId) || !int.TryParse(parts[1], out var agencyId))
                throw new ArgumentException($"Could not parse '{value}'", nameof(value));
                
            return Create(agentId, agencyId);
        }


        public static AgentInAgencyId Create(int agentId, int agencyId)
        {
            if (agentId.Equals(default))
                throw new ArgumentException($"Agent id cannot be {default(int)}");
            
            if (agencyId.Equals(default))
                throw new ArgumentException($"Agency id cannot be {default(int)}");
            
            return new AgentInAgencyId(agentId, agencyId);
        }


        private const string Delimiter = "-";
    }
}