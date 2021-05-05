using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Models.Users
{
    public static class UserExtensions
    {
        public static ApiCaller ToApiCaller(this ServiceAccount serviceAccount) => new(serviceAccount.Id, ApiCallerTypes.ServiceAccount);
        
        public static ApiCaller ToApiCaller(this AgentContext agentContext) => new(agentContext.AgentId, ApiCallerTypes.Agent);
        
        public static ApiCaller ToApiCaller(this Administrator administrator) => new(administrator.Id, ApiCallerTypes.Admin);
    }
}