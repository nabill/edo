using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IPermissionChecker
    {
        ValueTask<Result> CheckInAgencyPermission(AgentContext agent, InAgencyPermissions permission);
    }
}