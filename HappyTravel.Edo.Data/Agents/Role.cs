using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InAgencyPermissions Permissions { get; set; }
    }
}