using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Data.Infrastructure;

namespace HappyTravel.Edo.Api.Infrastructure.Invitations
{
    public interface IInvitationRecordService
    {
        Task<Result> Revoke(string code);

        Task<Result> SetToResent(string code);

        Task<Result> SetAccepted(string code);

        Task<Result<UserInvitation>> GetActiveInvitation(string code);

        UserInvitationData GetInvitationData(UserInvitation invitation);
    }
}
