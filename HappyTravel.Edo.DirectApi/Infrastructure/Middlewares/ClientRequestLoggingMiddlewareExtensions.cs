using Microsoft.AspNetCore.Builder;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Middlewares
{
    public static class ClientRequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseClientRequestLogging(this IApplicationBuilder builder) 
            => builder.UseMiddleware<ClientRequestLoggingMiddleware>();
    }
}