using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class LoggerExtensions
    {
        public static IDisposable AddSearchIdScope<T>(this ILogger<T> logger, Guid searchId)
        {
            return logger.BeginScope(new Dictionary<string, object>
            {
                { "SearchId", searchId }
            });
        }
    }
}