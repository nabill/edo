using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgentAgencyRelation
    {
        public int AgentId { get; set; }
        public int[] AgentRoleIds { get; set; }
        public int AgencyId { get; set; }
        public AgentAgencyRelationTypes Type { get; set; }
        public bool IsActive { get; set; }


        public override bool Equals(object obj) => obj is AgentAgencyRelation other && Equals(other);


        public bool Equals(AgentAgencyRelation other)
            => Equals((AgentId, AgencyId, Type),
                (other.AgentId, other.AgencyId, other.Type));


        public override int GetHashCode() => (AgentId, AgencyId, Type).GetHashCode();
    }
}