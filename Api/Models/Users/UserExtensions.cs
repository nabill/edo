using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Models.Users
{
    public static class UserExtensions
    {
        public static ApiCaller ToApiCaller(this ServiceAccount serviceAccount) => new(serviceAccount.Id.ToString(), ApiCallerTypes.ServiceAccount);
        
        public static ApiCaller ToApiCaller(this AgentContext agentContext) => new(agentContext.AgentId.ToString(), ApiCallerTypes.Agent);
        
        public static ApiCaller ToApiCaller(this Administrator administrator) => new(administrator.Id.ToString(), ApiCallerTypes.Admin);
    }
}