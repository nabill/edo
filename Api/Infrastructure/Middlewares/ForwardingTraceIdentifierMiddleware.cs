using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static HappyTravel.Edo.Api.Infrastructure.Constants.Common;

namespace HappyTravel.Edo.Api.Infrastructure.Middlewares;

public class ForwardingTraceIdentifierMiddleware
{
    public ForwardingTraceIdentifierMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add(TraceIdHeader, Activity.Current?.TraceId.ToString());
        return _next(context);
    }

    private readonly RequestDelegate _next;
}