using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.AdministratorServices.Invitations
{
    public interface IAdminInvitationCreateService
    {
        Task<Result<string>> Create(UserDescriptionInfo prefilledData, int inviterUserId);

        Task<Result<string>> Send(UserDescriptionInfo prefilledData, int inviterUserId);

        Task<Result<string>> Resend(string oldInvitationCode);
    }
}
