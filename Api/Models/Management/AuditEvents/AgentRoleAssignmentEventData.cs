using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgentRoleAssignmentEventData
    {
        public AgentRoleAssignmentEventData(int assignerAdministratorId, int assigneeAdministratorId, List<int> newRoles)
        {
            AssignerAdministratorId = assignerAdministratorId;
            AssigneeAdministratorId = assigneeAdministratorId;
            NewRoles = newRoles;
        }


        public int AssignerAdministratorId { get; }
        public int AssigneeAdministratorId { get; }
        public List<int> NewRoles { get; }
    }
}
