using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;

namespace HappyTravel.Edo.Api.AdministratorServices.Invitations
{
    public interface IAdminInvitationAcceptService
    {
        Task<Result> Accept(string invitationCode, UserInvitationData filledData, string identity, string email);
    }
}
