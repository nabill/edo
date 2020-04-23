using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AssignInCounterpartyPermissionsRequest
    {
        [JsonConstructor]
        public AssignInCounterpartyPermissionsRequest(List<InCounterpartyPermissions> permissions)
        {
            Permissions = permissions ?? new List<InCounterpartyPermissions>();
        }


        public List<InCounterpartyPermissions> Permissions { get; }
    }
}