using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentRoleInfo
    {
        public AgentRoleInfo(int id, string name, List<InAgencyPermissions> permissions)
        {
            Id = id;
            Name = name;
            Permissions = permissions ?? new List<InAgencyPermissions>();
        }
        
        public int Id { get; }
        public string Name { get; }
        public List<InAgencyPermissions>  Permissions { get; }
    }
}