using System;
using HappyTravel.Edo.Api.Infrastructure.Emails;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Payments;
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
                $"INFORMATION | {nameof(MailSender)}: {{message}}");
            InvitationCreatedEventOccured = LoggerMessage.Define<string>(LogLevel.Information, 
                new EventId((int) LoggerEvents.InvitationCreatedInformation, LoggerEvents.InvitationCreatedInformation.ToString()), 
                $"INFORMATION | {nameof(InvitationService)}: {{message}}");
            CustomerRegistrationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning, 
                new EventId((int) LoggerEvents.CustomerRegistrationFailed, LoggerEvents.CustomerRegistrationFailed.ToString()), 
                $"ERROR | {nameof(CustomerRegistrationService)}: {{message}}");
            CustomerRegistrationSuccessEventOccured= LoggerMessage.Define<string>(LogLevel.Information, 
                new EventId((int) LoggerEvents.CustomerRegistrationSuccess, LoggerEvents.CustomerRegistrationSuccess.ToString()), 
                $"INFORMATION | {nameof(CustomerRegistrationService)}: {{message}}");
            PayfortClientExceptionOccurred = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int)LoggerEvents.PayfortClientException, LoggerEvents.PayfortClientException.ToString()),
                $"CRITICAL | {nameof(PayfortService)}: ");
        }

        internal static void LogDataProviderClientException(this ILogger logger, Exception exception) => DataProviderClientExceptionOccurred(logger, exception);

        internal static void LogGeocoderException(this ILogger logger, Exception exception) => GeoCoderExceptionOccurred(logger, exception);

        internal static void LogSendMailException(this ILogger logger, Exception exception) =>
            SendMailExceptionOccurred(logger, exception);
        
        internal static void LogSendMailInformation(this ILogger logger, string message) =>
            SendMailEventOccured(logger, message, null);
        
        internal static void LogInvitationCreatedInformation(this ILogger logger, string message) =>
            InvitationCreatedEventOccured(logger, message, null);
        
        internal static void LogCustomerRegistrationFailed(this ILogger logger, string message) =>
            CustomerRegistrationFailedEventOccured(logger, message, null);
        
        internal static void LogCustomerRegistrationSuccess(this ILogger logger, string message) =>
            CustomerRegistrationSuccessEventOccured(logger, message, null);

        internal static void LogPayfortClientException(this ILogger logger, Exception exception) => PayfortClientExceptionOccurred(logger, exception);

        private static readonly Action<ILogger, Exception> DataProviderClientExceptionOccurred;
        private static readonly Action<ILogger, Exception> GeoCoderExceptionOccurred;
        private static readonly Action<ILogger, Exception> SendMailExceptionOccurred;
        private static readonly Action<ILogger, Exception> PayfortClientExceptionOccurred;
        private static readonly Action<ILogger, string, Exception> SendMailEventOccured;
        private static readonly Action<ILogger, string, Exception> InvitationCreatedEventOccured;
        private static readonly Action<ILogger, string, Exception> CustomerRegistrationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> CustomerRegistrationSuccessEventOccured;
    }
}