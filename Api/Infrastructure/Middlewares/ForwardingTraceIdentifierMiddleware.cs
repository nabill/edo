using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Infrastructure.Middlewares;

public class ForwardingTraceIdentifierMiddleware
{
    public ForwardingTraceIdentifierMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("TraceId", Activity.Current?.TraceId.ToString());
        return _next(context);
    }

    private readonly RequestDelegate _next;
}