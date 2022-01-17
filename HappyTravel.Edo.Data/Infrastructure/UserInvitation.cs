using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Infrastructure
{
    public class UserInvitation
    {
        public string CodeHash { get; set; }
        public string Email { get; set; }
        public DateTimeOffset Created { get; set; }
        public int InviterUserId { get; set; }
        public int? InviterAgencyId { get; set; }
        public UserInvitationStatuses InvitationStatus { get; set; }
        public UserInvitationTypes InvitationType { get; set; }
        public string Data { get; set; }
    }
}