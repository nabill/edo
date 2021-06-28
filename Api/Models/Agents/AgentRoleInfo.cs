using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentRoleInfo
    {
        public AgentRoleInfo(int id, string name, InAgencyPermissions permissions)
        {
            Id = id;
            Name = name;
            Permissions = permissions
                .ToList()
                .Select(x => x.ToString());
        }
        
        public int Id { get; }
        public string Name { get; }
        public IEnumerable<string>  Permissions { get; }
    }
}