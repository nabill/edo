using HappyTravel.Edo.Api.Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace HappyTravel.LogDelegatesGenerator
{
    public readonly struct LogEventDescriptor
    {
        public LogEventDescriptor(LoggerEvents @event, LogLevel level, string source, bool isException = false)
        {
            Event = @event;
            Level = level;
            Source = source;
            IsException = isException;
        }
        
        public LoggerEvents Event { get; }
        public LogLevel Level { get; }
        public string Source { get; }
        public bool IsException { get; }
        public string LogLevelUpperCase => Level.ToString().ToUpperInvariant();
    }
}