using System;
using Microsoft.Extensions.Logging;

namespace Api.Infrastructure.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            GeoCoderExceptionOccured = LoggerMessage.Define(LogLevel.Error,
                new EventId(1001, "GeoCoderException"),
                $"ERROR | GoogleGeoCoder: {{message}}");
            
            AvailabilityCheckExceptionOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1002, "AvailabilityCheckException"),
                $"ERROR | DataProviderClient: {{message}}");
            
            DataProviderClientExceptionOccured = LoggerMessage.Define(LogLevel.Critical,
                new EventId(1003, "DataProviderClientException"),
                $"CRITICAL | DataProviderClient: {{message}}");
            
            DataProviderRequestErrorOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1004, "DataProviderRequestError"),
                $"ERROR | DataProvider: {{message}}");
            
            InvitationCreatedOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1006, "InvitationCreated"),
                $"INFORMATION | AgentInvitationService: {{message}}");
            
            AgentRegistrationFailedOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1007, "AgentRegistrationFailed"),
                $"ERROR | AgentRegistrationService: {{message}}");
            
            AgentRegistrationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1008, "AgentRegistrationSuccess"),
                $"INFORMATION | AgentRegistrationService: {{message}}");
            
            PayfortClientExceptionOccured = LoggerMessage.Define(LogLevel.Critical,
                new EventId(1009, "PayfortClientException"),
                $"CRITICAL | PayfortService: {{message}}");
            
            PaymentAccountCreationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1010, "PaymentAccountCreationSuccess"),
                $"INFORMATION | AccountManagementService: {{message}}");
            
            PaymentAccountCreationFailedOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1011, "PaymentAccountCreationFailed"),
                $"ERROR | AccountManagementService: {{message}}");
            
            EntityLockFailedOccured = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId(1012, "EntityLockFailed"),
                $"CRITICAL | EntityLocker: {{message}}");
            
            PayfortErrorOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1013, "PayfortError"),
                $"ERROR | PayfortService: {{message}}");
            
            ExternalPaymentLinkSendSuccessOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1014, "ExternalPaymentLinkSendSuccess"),
                $"INFORMATION | PaymentLinkService: {{message}}");
            
            ExternalPaymentLinkSendFailedOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1015, "ExternalPaymentLinkSendFailed"),
                $"ERROR | PaymentLinkService: {{message}}");
            
            UnableGetBookingDetailsFromNetstormingXmlOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1017, "UnableGetBookingDetailsFromNetstormingXml"),
                $"WARNING | Test: {{message}}");
            
            UnableToAcceptNetstormingRequestOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1018, "UnableToAcceptNetstormingRequest"),
                $"WARNING | Test: {{message}}");
            
            BookingFinalizationFailureOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1020, "BookingFinalizationFailure"),
                $"ERROR | BookingService: {{message}}");
            
            BookingFinalizationPaymentFailureOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1021, "BookingFinalizationPaymentFailure"),
                $"WARNING | BookingService: {{message}}");
            
            BookingFinalizationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1022, "BookingFinalizationSuccess"),
                $"INFORMATION | BookingService: {{message}}");
            
            BookingResponseProcessFailureOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1030, "BookingResponseProcessFailure"),
                $"ERROR | BookingService: {{message}}");
            
            BookingResponseProcessSuccessOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1031, "BookingResponseProcessSuccess"),
                $"INFORMATION | BookingService: {{message}}");
            
            BookingResponseProcessStartedOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1032, "BookingResponseProcessStarted"),
                $"INFORMATION | BookingService: {{message}}");
            
            AdministratorAuthorizationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1100, "AdministratorAuthorizationSuccess"),
                $"DEBUG | AdministratorPermissionsAuthorizationHandler: {{message}}");
            
            AdministratorAuthorizationFailureOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1101, "AdministratorAuthorizationFailure"),
                $"WARNING | AdministratorPermissionsAuthorizationHandler: {{message}}");
            
            AgentAuthorizationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1110, "AgentAuthorizationSuccess"),
                $"DEBUG | InAgencyPermissionAuthorizationHandler: {{message}}");
            
            AgentAuthorizationFailureOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1111, "AgentAuthorizationFailure"),
                $"WARNING | InAgencyPermissionAuthorizationHandler: {{message}}");
            
            CounterpartyAccountCreationFailureOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1120, "CounterpartyAccountCreationFailure"),
                $"ERROR | AccountManagementService: {{message}}");
            
            CounterpartyAccountCreationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1121, "CounterpartyAccountCreationSuccess"),
                $"INFORMATION | AccountManagementService: {{message}}");
            
            ServiceAccountAuthorizationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1125, "ServiceAccountAuthorizationSuccess"),
                $"DEBUG | InAgencyPermissionAuthorizationHandler: {{message}}");
            
            ServiceAccountAuthorizationFailureOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1126, "ServiceAccountAuthorizationFailure"),
                $"WARNING | InAgencyPermissionAuthorizationHandler: {{message}}");
            
            LocationNormalizedOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1130, "LocationNormalized"),
                $"INFORMATION | LocationNormalizer: {{message}}");
            
            MultiProviderAvailabilitySearchStartedOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1140, "MultiProviderAvailabilitySearchStarted"),
                $"DEBUG | AvailabilitySearchScheduler: {{message}}");
            
            ProviderAvailabilitySearchStartedOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1141, "ProviderAvailabilitySearchStarted"),
                $"DEBUG | AvailabilitySearchScheduler: {{message}}");
            
            ProviderAvailabilitySearchSuccessOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1142, "ProviderAvailabilitySearchSuccess"),
                $"DEBUG | AvailabilitySearchScheduler: {{message}}");
            
            ProviderAvailabilitySearchFailureOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1143, "ProviderAvailabilitySearchFailure"),
                $"ERROR | AvailabilitySearchScheduler: {{message}}");
            
            ProviderAvailabilitySearchExceptionOccured = LoggerMessage.Define(LogLevel.Critical,
                new EventId(1145, "ProviderAvailabilitySearchException"),
                $"CRITICAL | AvailabilitySearchScheduler: {{message}}");
            
            CounterpartyStateAuthorizationSuccessOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1150, "CounterpartyStateAuthorizationSuccess"),
                $"DEBUG | MinCounterpartyStateAuthorizationHandler: {{message}}");
            
            CounterpartyStateAuthorizationFailureOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1151, "CounterpartyStateAuthorizationFailure"),
                $"WARNING | MinCounterpartyStateAuthorizationHandler: {{message}}");
            
            DefaultLanguageKeyIsMissingInFieldOfLocationsTableOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1200, "DefaultLanguageKeyIsMissingInFieldOfLocationsTable"),
                $"WARNING | LocationNormalizer: {{message}}");
            
        }
    
                
         internal static void LogGeoCoderException(this ILogger logger, Exception exception)
            => GeoCoderExceptionOccured(logger, exception);
                
         internal static void LogAvailabilityCheckException(this ILogger logger, string message)
            => AvailabilityCheckExceptionOccured(logger, message, null);
                
         internal static void LogDataProviderClientException(this ILogger logger, Exception exception)
            => DataProviderClientExceptionOccured(logger, exception);
                
         internal static void LogDataProviderRequestError(this ILogger logger, string message)
            => DataProviderRequestErrorOccured(logger, message, null);
                
         internal static void LogInvitationCreated(this ILogger logger, string message)
            => InvitationCreatedOccured(logger, message, null);
                
         internal static void LogAgentRegistrationFailed(this ILogger logger, string message)
            => AgentRegistrationFailedOccured(logger, message, null);
                
         internal static void LogAgentRegistrationSuccess(this ILogger logger, string message)
            => AgentRegistrationSuccessOccured(logger, message, null);
                
         internal static void LogPayfortClientException(this ILogger logger, Exception exception)
            => PayfortClientExceptionOccured(logger, exception);
                
         internal static void LogPaymentAccountCreationSuccess(this ILogger logger, string message)
            => PaymentAccountCreationSuccessOccured(logger, message, null);
                
         internal static void LogPaymentAccountCreationFailed(this ILogger logger, string message)
            => PaymentAccountCreationFailedOccured(logger, message, null);
                
         internal static void LogEntityLockFailed(this ILogger logger, string message)
            => EntityLockFailedOccured(logger, message, null);
                
         internal static void LogPayfortError(this ILogger logger, string message)
            => PayfortErrorOccured(logger, message, null);
                
         internal static void LogExternalPaymentLinkSendSuccess(this ILogger logger, string message)
            => ExternalPaymentLinkSendSuccessOccured(logger, message, null);
                
         internal static void LogExternalPaymentLinkSendFailed(this ILogger logger, string message)
            => ExternalPaymentLinkSendFailedOccured(logger, message, null);
                
         internal static void LogUnableGetBookingDetailsFromNetstormingXml(this ILogger logger, string message)
            => UnableGetBookingDetailsFromNetstormingXmlOccured(logger, message, null);
                
         internal static void LogUnableToAcceptNetstormingRequest(this ILogger logger, string message)
            => UnableToAcceptNetstormingRequestOccured(logger, message, null);
                
         internal static void LogBookingFinalizationFailure(this ILogger logger, string message)
            => BookingFinalizationFailureOccured(logger, message, null);
                
         internal static void LogBookingFinalizationPaymentFailure(this ILogger logger, string message)
            => BookingFinalizationPaymentFailureOccured(logger, message, null);
                
         internal static void LogBookingFinalizationSuccess(this ILogger logger, string message)
            => BookingFinalizationSuccessOccured(logger, message, null);
                
         internal static void LogBookingResponseProcessFailure(this ILogger logger, string message)
            => BookingResponseProcessFailureOccured(logger, message, null);
                
         internal static void LogBookingResponseProcessSuccess(this ILogger logger, string message)
            => BookingResponseProcessSuccessOccured(logger, message, null);
                
         internal static void LogBookingResponseProcessStarted(this ILogger logger, string message)
            => BookingResponseProcessStartedOccured(logger, message, null);
                
         internal static void LogAdministratorAuthorizationSuccess(this ILogger logger, string message)
            => AdministratorAuthorizationSuccessOccured(logger, message, null);
                
         internal static void LogAdministratorAuthorizationFailure(this ILogger logger, string message)
            => AdministratorAuthorizationFailureOccured(logger, message, null);
                
         internal static void LogAgentAuthorizationSuccess(this ILogger logger, string message)
            => AgentAuthorizationSuccessOccured(logger, message, null);
                
         internal static void LogAgentAuthorizationFailure(this ILogger logger, string message)
            => AgentAuthorizationFailureOccured(logger, message, null);
                
         internal static void LogCounterpartyAccountCreationFailure(this ILogger logger, string message)
            => CounterpartyAccountCreationFailureOccured(logger, message, null);
                
         internal static void LogCounterpartyAccountCreationSuccess(this ILogger logger, string message)
            => CounterpartyAccountCreationSuccessOccured(logger, message, null);
                
         internal static void LogServiceAccountAuthorizationSuccess(this ILogger logger, string message)
            => ServiceAccountAuthorizationSuccessOccured(logger, message, null);
                
         internal static void LogServiceAccountAuthorizationFailure(this ILogger logger, string message)
            => ServiceAccountAuthorizationFailureOccured(logger, message, null);
                
         internal static void LogLocationNormalized(this ILogger logger, string message)
            => LocationNormalizedOccured(logger, message, null);
                
         internal static void LogMultiProviderAvailabilitySearchStarted(this ILogger logger, string message)
            => MultiProviderAvailabilitySearchStartedOccured(logger, message, null);
                
         internal static void LogProviderAvailabilitySearchStarted(this ILogger logger, string message)
            => ProviderAvailabilitySearchStartedOccured(logger, message, null);
                
         internal static void LogProviderAvailabilitySearchSuccess(this ILogger logger, string message)
            => ProviderAvailabilitySearchSuccessOccured(logger, message, null);
                
         internal static void LogProviderAvailabilitySearchFailure(this ILogger logger, string message)
            => ProviderAvailabilitySearchFailureOccured(logger, message, null);
                
         internal static void LogProviderAvailabilitySearchException(this ILogger logger, Exception exception)
            => ProviderAvailabilitySearchExceptionOccured(logger, exception);
                
         internal static void LogCounterpartyStateAuthorizationSuccess(this ILogger logger, string message)
            => CounterpartyStateAuthorizationSuccessOccured(logger, message, null);
                
         internal static void LogCounterpartyStateAuthorizationFailure(this ILogger logger, string message)
            => CounterpartyStateAuthorizationFailureOccured(logger, message, null);
                
         internal static void LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(this ILogger logger, string message)
            => DefaultLanguageKeyIsMissingInFieldOfLocationsTableOccured(logger, message, null);
    
    
        
        private static readonly Action<ILogger, Exception> GeoCoderExceptionOccured;
        
        private static readonly Action<ILogger, string, Exception> AvailabilityCheckExceptionOccured;
        
        private static readonly Action<ILogger, Exception> DataProviderClientExceptionOccured;
        
        private static readonly Action<ILogger, string, Exception> DataProviderRequestErrorOccured;
        
        private static readonly Action<ILogger, string, Exception> InvitationCreatedOccured;
        
        private static readonly Action<ILogger, string, Exception> AgentRegistrationFailedOccured;
        
        private static readonly Action<ILogger, string, Exception> AgentRegistrationSuccessOccured;
        
        private static readonly Action<ILogger, Exception> PayfortClientExceptionOccured;
        
        private static readonly Action<ILogger, string, Exception> PaymentAccountCreationSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> PaymentAccountCreationFailedOccured;
        
        private static readonly Action<ILogger, string, Exception> EntityLockFailedOccured;
        
        private static readonly Action<ILogger, string, Exception> PayfortErrorOccured;
        
        private static readonly Action<ILogger, string, Exception> ExternalPaymentLinkSendSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> ExternalPaymentLinkSendFailedOccured;
        
        private static readonly Action<ILogger, string, Exception> UnableGetBookingDetailsFromNetstormingXmlOccured;
        
        private static readonly Action<ILogger, string, Exception> UnableToAcceptNetstormingRequestOccured;
        
        private static readonly Action<ILogger, string, Exception> BookingFinalizationFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> BookingFinalizationPaymentFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> BookingFinalizationSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> BookingResponseProcessFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> BookingResponseProcessSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> BookingResponseProcessStartedOccured;
        
        private static readonly Action<ILogger, string, Exception> AdministratorAuthorizationSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> AdministratorAuthorizationFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> AgentAuthorizationSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> AgentAuthorizationFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> CounterpartyAccountCreationFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> CounterpartyAccountCreationSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> ServiceAccountAuthorizationSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> ServiceAccountAuthorizationFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> LocationNormalizedOccured;
        
        private static readonly Action<ILogger, string, Exception> MultiProviderAvailabilitySearchStartedOccured;
        
        private static readonly Action<ILogger, string, Exception> ProviderAvailabilitySearchStartedOccured;
        
        private static readonly Action<ILogger, string, Exception> ProviderAvailabilitySearchSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> ProviderAvailabilitySearchFailureOccured;
        
        private static readonly Action<ILogger, Exception> ProviderAvailabilitySearchExceptionOccured;
        
        private static readonly Action<ILogger, string, Exception> CounterpartyStateAuthorizationSuccessOccured;
        
        private static readonly Action<ILogger, string, Exception> CounterpartyStateAuthorizationFailureOccured;
        
        private static readonly Action<ILogger, string, Exception> DefaultLanguageKeyIsMissingInFieldOfLocationsTableOccured;
    }
}