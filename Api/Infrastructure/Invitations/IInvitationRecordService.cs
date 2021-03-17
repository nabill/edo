using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Data.Infrastructure;

namespace HappyTravel.Edo.Api.Infrastructure.Invitations
{
    public interface IInvitationRecordService
    {
        Task<Result> Revoke(string codeHash);

        Task<Result> SetToResent(string codeHash);

        Task<Result> SetAccepted(string code);

        Task<Result<UserInvitation>> GetActiveInvitationByHash(string codeHash);

        Task<Result<UserInvitation>> GetActiveInvitationByCode(string code);

        UserInvitationData GetInvitationData(UserInvitation invitation);
    }
}
