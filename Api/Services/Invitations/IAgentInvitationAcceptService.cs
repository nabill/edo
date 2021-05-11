using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Invitations;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public interface IAgentInvitationAcceptService
    {
        Task<Result> Accept(string invitationCode, UserInvitationData filledData, string identity, string email);
    }
}
