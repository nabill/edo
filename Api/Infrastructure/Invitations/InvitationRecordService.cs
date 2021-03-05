using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Infrastructure.Invitations
{
    public class InvitationRecordService : IInvitationRecordService
    {
        public InvitationRecordService(
            IDateTimeProvider dateTimeProvider,
            EdoContext context)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<Result> Revoke(string code)
        {
            return await GetActiveInvitation(code)
                .Tap(SaveRevoked);


            Task SaveRevoked(UserInvitation invitation)
            {
                invitation.InvitationStatus = UserInvitationStatuses.Revoked;
                _context.Update(invitation);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> SetResent(string code)
        {
            return await GetActiveInvitation(code)
                .Tap(SaveResent);


            Task SaveResent(UserInvitation oldInvitation)
            {
                oldInvitation.InvitationStatus = UserInvitationStatuses.Resent;
                _context.Update(oldInvitation);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> SetAccepted(string code)
        {
            return await GetActiveInvitation(code)
                .Tap(SaveAccepted);


            Task SaveAccepted(UserInvitation invitation)
            {
                invitation.InvitationStatus = UserInvitationStatuses.Accepted;
                _context.Update(invitation);
                return _context.SaveChangesAsync();
            }
        }


        public Task<Result<UserInvitation>> GetActiveInvitation(string code)
        {
            return GetInvitation()
                .Ensure(InvitationIsActual, "Invitation expired");


            async Task<Result<UserInvitation>> GetInvitation()
            {
                var invitation = await _context.UserInvitations
                    .SingleOrDefaultAsync(i
                        => i.CodeHash == HashGenerator.ComputeSha256(code)
                        && i.InvitationStatus == UserInvitationStatuses.Active);

                return invitation ?? Result.Failure<UserInvitation>("Invitation with specified code either does not exist, or is not active.");
            }


            bool InvitationIsActual(UserInvitation invitation)
                => invitation.Created + _invitationExpirationPeriod > _dateTimeProvider.UtcNow();
        }


        public UserInvitationData GetInvitationData(UserInvitation invitation)
            => JsonConvert.DeserializeObject<UserInvitationData>(invitation.Data);


        private readonly TimeSpan _invitationExpirationPeriod = TimeSpan.FromDays(7);

        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}
