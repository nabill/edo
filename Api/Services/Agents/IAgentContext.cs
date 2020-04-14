using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentContext
    {
        [Obsolete("Use GetAgent instead")]
        ValueTask<Result<AgentInfo>> GetAgentInfo();
        
        ValueTask<AgentInfo> GetAgent();

        Task<Result<UserInfo>> GetUserInfo();

        Task<List<AgentCounterpartyInfo>> GetAgentCounterparties();
        
        ValueTask<Result<AgentInfo>> SetAgentInfo(int agentId);
    }
}