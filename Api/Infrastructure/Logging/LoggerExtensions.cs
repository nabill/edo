using System;
using HappyTravel.Edo.Api.Services.Locations;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            DataProviderClientExceptionOccurred = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int) LoggerEvents.NetClientException, LoggerEvents.NetClientException.ToString()),
                $"CRITICAL | {nameof(DataProviderClient)}: ");
            GeoCoderExceptionOccurred = LoggerMessage.Define(LogLevel.Error,
                new EventId((int) LoggerEvents.GeocoderException, LoggerEvents.GeocoderException.ToString()),
                $"EXCEPTION | {nameof(GoogleGeoCoder)}: ");
        }


        internal static void LogDataProviderClientException(this ILogger logger, Exception exception) => DataProviderClientExceptionOccurred(logger, exception);

        internal static void LogGeocoderException(this ILogger logger, Exception exception) => GeoCoderExceptionOccurred(logger, exception);


        private static readonly Action<ILogger, Exception> DataProviderClientExceptionOccurred;
        private static readonly Action<ILogger, Exception> GeoCoderExceptionOccurred;
    }
}