using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.DirectApi.Services.Overriden
{
    public class EdoDummyNotificationService : INotificationService
    {
        public Task<Result> Send(ApiCaller apiCaller, JsonDocument message, NotificationTypes notificationType) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(SlimAdminContext admin, JsonDocument message, NotificationTypes notificationType) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(SlimAdminContext admin, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(SlimAdminContext admin, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails) 
            => Task.FromResult(Result.Success());


        public Task<Result> Send(DataWithCompanyInfo messageData, NotificationTypes notificationType, string email) 
            => Task.FromResult(Result.Success());


        public Task<List<SlimNotification>> Get(SlimAgentContext agent, int skip, int top) 
            => Task.FromResult(new List<SlimNotification>(0));


        public Task<List<SlimNotification>> Get(SlimAdminContext admin, int skip, int top) 
            => Task.FromResult(new List<SlimNotification>(0));
    }
}