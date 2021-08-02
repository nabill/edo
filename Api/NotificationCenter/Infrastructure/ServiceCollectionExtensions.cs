using HappyTravel.Edo.Api.NotificationCenter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.NotificationCenter.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationCenter(this IServiceCollection services, string redisConnectionString)
        {
            services.AddSignalR().AddStackExchangeRedis(redisConnectionString);
            services.AddTransient<IInternalNotificationService, InternalNotificationService>();
            services.AddTransient<INotificationOptionsService, NotificationOptionsService>();
            services.AddTransient<INotificationService, NotificationService>();

            return services;
        }
    }
}