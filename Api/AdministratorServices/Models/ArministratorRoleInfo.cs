using HappyTravel.Edo.Common.Enums.Administrators;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public readonly struct AdministratorRoleInfo
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public AdministratorPermissions Permissions { get; init; }
    }
}
