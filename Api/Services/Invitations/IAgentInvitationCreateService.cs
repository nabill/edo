using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public interface IAgentInvitationCreateService
    {
        Task<Result<string>> Create(UserInvitationData prefilledData, UserInvitationTypes invitationType,
            int inviterUserId, int? inviterAgencyId = null);

        Task<Result<string>> Send(UserInvitationData prefilledData, UserInvitationTypes invitationType,
            int inviterUserId, int? inviterAgencyId = null);

        Task<Result<string>> Resend(string oldInvitationCode);
    }
}