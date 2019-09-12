using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IUserInvitationService
    {
        Task<Result> SendInvitation<TInvitationData>(string email, TInvitationData invitationInfo, string mailTemplateId);
        Task AcceptInvitation(string invitationCode);
        Task<Result<TInvitationData>> GetPendingInvitation<TInvitationData>(string invitationCode);
    }
}