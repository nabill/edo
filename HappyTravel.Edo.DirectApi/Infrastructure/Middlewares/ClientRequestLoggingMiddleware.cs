using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Middlewares
{
    public class ClientRequestLoggingMiddleware
    {
        public ClientRequestLoggingMiddleware(RequestDelegate next, ILogger<ClientRequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        
        public Task InvokeAsync(HttpContext context, ITokenInfoAccessor tokenInfoAccessor)
        {
            var scopedData = new Dictionary<string, object>
            {
                {"ClientId", tokenInfoAccessor.GetClientId()}
            };
                
            using var disposable = _logger.BeginScope(scopedData);
            return _next(context);
        }
        
        
        private readonly RequestDelegate _next;
        private readonly ILogger<ClientRequestLoggingMiddleware> _logger;
    }
}