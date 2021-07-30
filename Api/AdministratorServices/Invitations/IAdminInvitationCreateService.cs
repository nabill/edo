using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;

namespace HappyTravel.Edo.Api.AdministratorServices.Invitations
{
    public interface IAdminInvitationCreateService
    {
        Task<Result<string>> Create(UserInvitationData prefilledData, int inviterUserId);

        Task<Result<string>> Send(UserInvitationData prefilledData, int inviterUserId);

        Task<Result<string>> Resend(string oldInvitationCodeHash);
    }
}
