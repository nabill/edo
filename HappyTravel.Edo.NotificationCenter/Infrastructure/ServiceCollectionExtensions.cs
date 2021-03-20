using HappyTravel.Edo.NotificationCenter.Services.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.NotificationCenter.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationCenter(this IServiceCollection services)
        {
            services.AddTransient<INotificationService, NotificationService>();
            services.AddSignalR();

            return services;
        }
    }
}