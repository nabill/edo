using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct AssignInCompanyPermissionsRequest
    {
        [JsonConstructor]
        public AssignInCompanyPermissionsRequest(List<InCompanyPermissions> inCompanyPermissions)
        {
            InCompanyPermissions = inCompanyPermissions;
        }
        
        public List<InCompanyPermissions> InCompanyPermissions { get; }
    }
}