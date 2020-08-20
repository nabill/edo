using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public static class PermissionSets
    {
        public const InAgencyPermissions Default = InAgencyPermissions.AccommodationAvailabilitySearch | 
            InAgencyPermissions.ObserveBalance |
            InAgencyPermissions.AgentInvitation; 

        public const InAgencyPermissions Master = InAgencyPermissions.All;
    }
}
