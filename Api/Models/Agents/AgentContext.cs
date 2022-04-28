using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentContext
    {
        public AgentContext(int agentId, string firstName, string lastName, string email,
            string title, string position, int agencyId, string agencyName, bool isMaster,
            InAgencyPermissions inAgencyPermissions, string countryHtId, string localityHtId,
            string countryCode, int marketId, List<int> agencyAncestors)
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
            InAgencyPermissions = inAgencyPermissions;
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
            CountryCode = countryCode;
            MarketId = marketId;
            AgencyAncestors = agencyAncestors;
        }


        public string AgentName => $"{Title}. {FirstName} {LastName}";


        public void Deconstruct(out int agentId, out int agencyId, out string agencyName, out bool isMaster)
        {
            agentId = AgentId;
            agencyId = AgencyId;
            agencyName = AgencyName;
            isMaster = IsMaster;
        }


        public bool Equals(AgentContext other)
            => (AgentId, AgencyId, IsMaster)
                == (other.AgentId, other.AgencyId, other.IsMaster);


        public override bool Equals(object obj) => obj is AgentContext other && Equals(other);


        public override int GetHashCode() => (AgentId, AgencyId, IsMaster).GetHashCode();


        public int AgentId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public int AgencyId { get; }
        public string AgencyName { get; }
        public bool IsMaster { get; }
        public InAgencyPermissions InAgencyPermissions { get; }
        public string CountryHtId { get; }
        public string LocalityHtId { get; }
        public string CountryCode { get; }
        public int MarketId { get; }
        public List<int> AgencyAncestors { get; }
        public string Title { get; }
        public string Position { get; }
    }
}