using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Models.Management
{
    public interface IAdministratorInvitationService
    {
        Task<Result> SendInvitation(AdministratorInvitationInfo invitationInfo);
        Task AcceptInvitation(string invitationCode);
        Task<Result<AdministratorInvitationInfo>> GetPendingInvitation(string invitationCode);
    }
}