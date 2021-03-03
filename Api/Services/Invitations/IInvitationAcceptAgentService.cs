using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public interface IInvitationAcceptAgentService
    {
        Task<Result> Accept(string invitationCode, UserInvitationData filledData, string identity);
    }
}
