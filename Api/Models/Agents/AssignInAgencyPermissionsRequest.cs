using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AssignInAgencyPermissionsRequest
    {
        [JsonConstructor]
        public AssignInAgencyPermissionsRequest(List<InAgencyPermissions> permissions)
        {
            Permissions = permissions ?? new List<InAgencyPermissions>();
        }


        public List<InAgencyPermissions> Permissions { get; }
    }
}