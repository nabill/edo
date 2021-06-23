using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public readonly struct AgentRoleInfo
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public InAgencyPermissions Permissions { get; init; }
    }
}
