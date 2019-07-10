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
            AvailabilityCheckErrorOccurred = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int) LoggerEvents.AvailabilityCheckException, LoggerEvents.AvailabilityCheckException.ToString()),
                $"CRITICAL | {nameof(AvailabilityService)}: ");
            GeoCoderErrorOccurred = LoggerMessage.Define(LogLevel.Error, new EventId((int) LoggerEvents.GeocoderException, LoggerEvents.GeocoderException.ToString()),
                $"EXCEPTION | {nameof(GoogleGeocoder)}: ");
        }


        internal static void LogAvailabilityCheckException(this ILogger logger, Exception exception) => AvailabilityCheckErrorOccurred(logger, exception);
        internal static void LogGeocoderException(this ILogger logger, Exception exception) => GeoCoderErrorOccurred(logger, exception);


        private static readonly Action<ILogger, Exception> AvailabilityCheckErrorOccurred;
        private static readonly Action<ILogger, Exception> GeoCoderErrorOccurred;
    }
}
