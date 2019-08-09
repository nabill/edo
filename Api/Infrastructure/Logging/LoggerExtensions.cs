using System;
using HappyTravel.Edo.Api.Services.Availabilities;
using HappyTravel.Edo.Api.Services.Locations;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            AvailabilityCheckExceptionOccurred = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int) LoggerEvents.AvailabilityCheckException, LoggerEvents.AvailabilityCheckException.ToString()),
                $"CRITICAL | {nameof(AvailabilityService)}: ");
            GeoCoderExceptionOccurred = LoggerMessage.Define(LogLevel.Error, new EventId((int) LoggerEvents.GeocoderException, LoggerEvents.GeocoderException.ToString()),
                $"EXCEPTION | {nameof(GoogleGeoCoder)}: ");
            NetClientExceptionOccurred = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int) LoggerEvents.NetClientException, LoggerEvents.NetClientException.ToString()),
                $"CRITICAL | {nameof(NetClient)}: ");
        }


        internal static void LogAvailabilityCheckException(this ILogger logger, Exception exception) => AvailabilityCheckExceptionOccurred(logger, exception);
        internal static void LogGeocoderException(this ILogger logger, Exception exception) => GeoCoderExceptionOccurred(logger, exception);
        internal static void LogNetClientException(this ILogger logger, Exception exception) => NetClientExceptionOccurred(logger, exception);


        private static readonly Action<ILogger, Exception> AvailabilityCheckExceptionOccurred;
        private static readonly Action<ILogger, Exception> GeoCoderExceptionOccurred;
        private static readonly Action<ILogger, Exception> NetClientExceptionOccurred;
    }
}
