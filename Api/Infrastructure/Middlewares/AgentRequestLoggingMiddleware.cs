using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Middlewares
{
    public class AgentRequestLoggingMiddleware
    {
        public AgentRequestLoggingMiddleware(RequestDelegate next, IAgentContextService agentContextService, ILogger<AgentRequestLoggingMiddleware> logger)
        {
            _next = next;
            _agentContextService = agentContextService;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            var scopedData = new Dictionary<string, object>();

            try
            {
                var agentContext = await _agentContextService.GetAgent();
                scopedData.Add("AgentId", agentContext.AgentId);
                scopedData.Add("AgencyId", agentContext.AgencyId);
            }
            catch (Exception)
            {
                // request without agent context
            }

            using var disposable = _logger.BeginScope(scopedData);
            await _next(context);
        }
        
        
        private readonly RequestDelegate _next;
        private readonly IAgentContextService _agentContextService;
        private readonly ILogger<AgentRequestLoggingMiddleware> _logger;
    }
}