using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgentCounterpartyRelation
    {
        public int AgentId { get; set; }
        public InCounterpartyPermissions InCounterpartyPermissions { get; set; }
        public int AgencyId { get; set; }
        public AgentCounterpartyRelationTypes Type { get; set; }


        public override bool Equals(object obj) => obj is AgentCounterpartyRelation other && Equals(other);


        public bool Equals(AgentCounterpartyRelation other)
            => Equals((AgentId, InCounterpartyPermissions, AgencyId, Type),
                (other.AgentId, other.InCounterpartyPermissions, other.AgencyId, other.Type));


        public override int GetHashCode() => (AgentId, InCounterpartyPermissions, AgencyId, Type).GetHashCode();
    }
}