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
        public AgentRequestLoggingMiddleware(RequestDelegate next, IAgentContextInternal agentContextService, ILogger<AgentRequestLoggingMiddleware> logger)
        {
            _next = next;
            _agentContextService = agentContextService;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            var scopedData = new Dictionary<string, object>();
            var (isSuccess, _, agentContext, _) = await _agentContextService.GetAgentInfo();

            if (isSuccess)
            {
                scopedData.Add("AgentId", agentContext.AgentId);
                scopedData.Add("AgencyId", agentContext.AgencyId);
            }

            using var disposable = _logger.BeginScope(scopedData);
            await _next(context);
        }
        
        
        private readonly RequestDelegate _next;
        private readonly IAgentContextInternal _agentContextService;
        private readonly ILogger<AgentRequestLoggingMiddleware> _logger;
    }
}