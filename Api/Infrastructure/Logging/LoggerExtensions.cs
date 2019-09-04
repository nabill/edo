using System;
using HappyTravel.Edo.Api.Infrastructure.Emails;
using HappyTravel.Edo.Api.Services.Locations;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            DataProviderClientExceptionOccurred = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int) LoggerEvents.DataProviderClientException, LoggerEvents.DataProviderClientException.ToString()),
                $"CRITICAL | {nameof(DataProviderClient)}: ");
            GeoCoderExceptionOccurred = LoggerMessage.Define(LogLevel.Error,
                new EventId((int) LoggerEvents.GeocoderException, LoggerEvents.GeocoderException.ToString()),
                $"EXCEPTION | {nameof(GoogleGeoCoder)}: ");
            SendMailExceptionOccurred = LoggerMessage.Define(LogLevel.Error,
                new EventId((int) LoggerEvents.SendMailException, LoggerEvents.SendMailException.ToString()),
                $"EXCEPTION | {nameof(MailSender)}: ");
            SendMailEventOccured = LoggerMessage.Define<string>(LogLevel.Information, 
                new EventId((int) LoggerEvents.SendMailInformation, LoggerEvents.SendMailInformation.ToString()), 
                $"INFORMATION | {nameof(MailSender)}: ");
        }

        internal static void LogDataProviderClientException(this ILogger logger, Exception exception) => DataProviderClientExceptionOccurred(logger, exception);

        internal static void LogGeocoderException(this ILogger logger, Exception exception) => GeoCoderExceptionOccurred(logger, exception);

        internal static void LogSendMailException(this ILogger logger, Exception exception) =>
            SendMailExceptionOccurred(logger, exception);
        
        internal static void LogSendMailInformation(this ILogger logger, string message) =>
            SendMailEventOccured(logger, message, null);
        
        private static readonly Action<ILogger, Exception> DataProviderClientExceptionOccurred;
        private static readonly Action<ILogger, Exception> GeoCoderExceptionOccurred;
        private static readonly Action<ILogger, Exception> SendMailExceptionOccurred;
        private static readonly Action<ILogger, string, Exception> SendMailEventOccured;
    }
}