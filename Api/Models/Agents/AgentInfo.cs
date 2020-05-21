using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentInfo
    {
        public AgentInfo(int agentId, string firstName, string lastName, string email,
            string title, string position, int counterpartyId, string counterpartyName, int agencyId, bool isMaster,
            InAgencyPermissions inAgencyPermissions)
        {
            AgentId = agentId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Title = title;
            Position = position;
            CounterpartyId = counterpartyId;
            CounterpartyName = counterpartyName;
            AgencyId = agencyId;
            IsMaster = isMaster;
            InAgencyPermissions = inAgencyPermissions;
        }


        public void Deconstruct(out int agentId, out int counterpartyId, out int agencyId, out bool isMaster)
        {
            agentId = AgentId;
            counterpartyId = CounterpartyId;
            agencyId = AgencyId;
            isMaster = IsMaster;
        }


        public bool Equals(AgentInfo other)
            => (AgentId, CounterpartyId: CounterpartyId, AgencyId, IsMaster)
                == (other.AgentId, other.CounterpartyId, other.AgencyId, other.IsMaster);


        public override bool Equals(object obj) => obj is AgentInfo other && Equals(other);


        public override int GetHashCode() => (AgentId, CounterpartyId: CounterpartyId, AgencyId, IsMaster).GetHashCode();


        public int AgentId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public int CounterpartyId { get; }
        public string CounterpartyName { get; }
        public int AgencyId { get; }
        public bool IsMaster { get; }
        public InAgencyPermissions InAgencyPermissions { get; }
        public string Title { get; }
        public string Position { get; }
    }
}