using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.MailSender.Infrastructure.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            SendMailExceptionOccurred = LoggerMessage.Define(LogLevel.Error,
                GetEventId(LoggerEvents.SendMailException),
                $"EXCEPTION | MailSender: ");
            SendMailEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                GetEventId(LoggerEvents.SendMailInformation),
                $"INFORMATION | MailSender: {{message}}");
        }


        internal static void LogSendMailException(this ILogger logger, Exception exception) 
            => SendMailExceptionOccurred(logger, exception);


        internal static void LogSendMailInformation(this ILogger logger, string message) 
            => SendMailEventOccured(logger, message, null);


        private static EventId GetEventId(LoggerEvents @event) 
            => new EventId((int) @event, @event.ToString());


        private static readonly Action<ILogger, Exception> SendMailExceptionOccurred;
        private static readonly Action<ILogger, string, Exception> SendMailEventOccured;
    }
}