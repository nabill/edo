using Microsoft.AspNetCore.Builder;

namespace HappyTravel.Edo.Api.Infrastructure.Middlewares;

public static class ForwardingTraceIdentifierMiddlewareExtensions
{
    public static IApplicationBuilder UseForwardingTraceIdentifier(this IApplicationBuilder builder) 
        => builder.UseMiddleware<ForwardingTraceIdentifierMiddleware>();
}