using HappyTravel.Edo.NotificationCenter.Services.Hub;
using HappyTravel.Edo.NotificationCenter.Services.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.NotificationCenter.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationCenter(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddTransient<SignalRSender>();
            services.AddTransient<INotificationService, NotificationService>();

            return services;
        }
    }
}