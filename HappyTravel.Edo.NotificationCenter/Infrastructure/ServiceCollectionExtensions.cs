using HappyTravel.Edo.NotificationCenter.Services.Message;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.NotificationCenter.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationCenter(this IServiceCollection services)
        {
            services.AddTransient<IMessageService, MessageService>();
            services.AddSignalR();

            return services;
        }
    }
}