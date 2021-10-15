using Microsoft.AspNetCore.Builder;

namespace HappyTravel.Edo.Api.Infrastructure.Middlewares
{
    public static class AgentRequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseAgentRequestLogging(this IApplicationBuilder builder) 
            => builder.UseMiddleware<AgentRequestLoggingMiddleware>();
    }
}