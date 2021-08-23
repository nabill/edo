using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct MoveAgentToAgencyRequest
    {
        public MoveAgentToAgencyRequest(int targetAgency, List<int> roleIds)
        {
            TargetAgency = targetAgency;
            RoleIds = roleIds ?? new List<int>();
        }
        
        public int TargetAgency { get; }
        public List<int> RoleIds { get; }
    }
}