using System;
using HappyTravel.Edo.Data.Infrastructure;

namespace HappyTravel.Edo.Api.Infrastructure.Invitations
{
    public static class InvitationExtensions
    {
        public static bool IsExpired(this UserInvitation invitation, DateTimeOffset date) 
            => invitation.Created + InvitationExpirationPeriod < date;

        
        private static readonly TimeSpan InvitationExpirationPeriod = TimeSpan.FromDays(7);
    }
}