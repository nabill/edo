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
        ValueTask<AgentInfo> GetAgent();

        Task<Result<UserInfo>> GetUserInfo();

        Task<List<AgentAgencyInfo>> GetAgentCounterparties();
        
        ValueTask<Result<AgentInfo>> SetAgentInfo(int agentId);
    }
}