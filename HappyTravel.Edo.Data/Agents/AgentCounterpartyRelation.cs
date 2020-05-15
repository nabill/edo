using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgentCounterpartyRelation
    {
        public int AgentId { get; set; }
        public InAgencyPermissions InAgencyPermissions { get; set; }
        public int AgencyId { get; set; }
        public AgentAgencyRelationTypes Type { get; set; }


        public override bool Equals(object obj) => obj is AgentCounterpartyRelation other && Equals(other);


        public bool Equals(AgentCounterpartyRelation other)
            => Equals((AgentId, InAgencyPermissions, AgencyId, Type),
                (other.AgentId, other.InAgencyPermissions, other.AgencyId, other.Type));


        public override int GetHashCode() => (AgentId, InCounterpartyPermissions: InAgencyPermissions, AgencyId, Type).GetHashCode();
    }
}