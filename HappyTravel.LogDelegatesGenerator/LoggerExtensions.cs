using System;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace HappyTravel.LogDelegatesGenerator
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            GeocoderExceptionOccured = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int) LoggerEvents.GeocoderException, LoggerEvents.GeocoderException.ToString()),
                $" | GoogleGeoCoder: ");
            
            DataProviderRequestErrorOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.DataProviderRequestError, LoggerEvents.DataProviderRequestError.ToString()),
                $" | DataProvider: ");
            
        }
    
                
         internal static void LogGeocoderException(this ILogger logger, Exception exception)
            => GeocoderExceptionOccured(logger, exception);
                
         internal static void LogDataProviderRequestError(this ILogger logger, string message)
            => DataProviderRequestErrorOccured(logger, message, null);
    
    
        
        private static readonly Action<ILogger, Exception> GeocoderExceptionOccured;
        
        private static readonly Action<ILogger, string, Exception> DataProviderRequestErrorOccured;
    }
}