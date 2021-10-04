using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Middlewares
{
    public class AgentRequestLoggingMiddleware
    {
        public AgentRequestLoggingMiddleware(RequestDelegate next, ILogger<AgentRequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context, IAgentContextInternal agentContextService)
        {
            var (isSuccess, _, agentContext, _) = await agentContextService.GetAgentInfo();

            if (isSuccess)
            {
                var scopedData = new Dictionary<string, object>
                {
                    {"AgentId", agentContext.AgentId},
                    {"AgencyId", agentContext.AgencyId}
                };
                
                using var disposable = _logger.BeginScope(scopedData);
                await _next(context);
            }
            else
            {
                await _next(context);
            }
        }
        
        
        private readonly RequestDelegate _next;
        private readonly ILogger<AgentRequestLoggingMiddleware> _logger;
    }
}