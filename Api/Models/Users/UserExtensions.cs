using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Models.Users
{
    public static class UserExtensions
    {
        public static UserInfo ToUserInfo(this ServiceAccount serviceAccount) => new UserInfo(serviceAccount.Id, UserTypes.ServiceAccount);
        
        public static UserInfo ToUserInfo(this AgentContext agentContext) => new UserInfo(agentContext.AgentId, UserTypes.Agent);
        
        public static UserInfo ToUserInfo(this Administrator administrator) => new UserInfo(administrator.Id, UserTypes.Admin);
    }
}