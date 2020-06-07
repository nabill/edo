using System;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
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
            DataProviderRequestErrorOccurred = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.DataProviderRequestError, LoggerEvents.DataProviderRequestError.ToString()),
                $"ERROR | {nameof(DataProvider)}: {{message}}");
            GeoCoderExceptionOccurred = LoggerMessage.Define(LogLevel.Error,
                new EventId((int) LoggerEvents.GeocoderException, LoggerEvents.GeocoderException.ToString()),
                $"EXCEPTION | {nameof(GoogleGeoCoder)}: ");
            InvitationCreatedEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.InvitationCreatedInformation, LoggerEvents.InvitationCreatedInformation.ToString()),
                $"INFORMATION | {nameof(AgentInvitationService)}: {{message}}");
            AgentRegistrationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.AgentRegistrationFailed, LoggerEvents.AgentRegistrationFailed.ToString()),
                $"ERROR | {nameof(AgentRegistrationService)}: {{message}}");
            AgentRegistrationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.AgentRegistrationSuccess, LoggerEvents.AgentRegistrationSuccess.ToString()),
                $"INFORMATION | {nameof(AgentRegistrationService)}: {{message}}");
            PaymentAccountCreationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.PaymentAccountCreationFailed, LoggerEvents.PaymentAccountCreationFailed.ToString()),
                $"ERROR | {nameof(AccountManagementService)}: {{message}}");
            PaymentAccountCreatedSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.PaymentAccountCreationSuccess, LoggerEvents.PaymentAccountCreationSuccess.ToString()),
                $"INFORMATION | {nameof(AccountManagementService)}: {{message}}");

            EntityLockFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId((int) LoggerEvents.EntityLockFailed, LoggerEvents.EntityLockFailed.ToString()),
                $"ERROR | {nameof(EntityLocker)}: {{message}}");
            PayfortClientExceptionOccurred = LoggerMessage.Define(LogLevel.Critical,
                new EventId((int) LoggerEvents.PayfortClientException, LoggerEvents.PayfortClientException.ToString()),
                $"CRITICAL | {nameof(PayfortService)}: ");
            PayfortErrorOccurred = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.PayfortError, LoggerEvents.PayfortError.ToString()),
                $"ERROR | {nameof(PayfortService)}: {{message}}");

            ExternalPaymentLinkSendSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.ExternalPaymentLinkSendSuccess, LoggerEvents.ExternalPaymentLinkSendSuccess.ToString()),
                $"INFORMATION | {nameof(PaymentLinkService)}: {{message}}");

            ExternalPaymentLinkSendFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.ExternalPaymentLinkSendFailed, LoggerEvents.ExternalPaymentLinkSendFailed.ToString()),
                $"ERROR | {nameof(PaymentLinkService)}: {{message}}");

            UnableCaptureWholeAmountForBookingEventOccured = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId((int) LoggerEvents.UnableCaptureWholeAmountForBooking, LoggerEvents.UnableCaptureWholeAmountForBooking.ToString()),
                $"CRITICAL | {nameof(PaymentLinkService)}: {{message}}");
            
            UnableToGetBookingDetailsFromNetstormingXmlEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.UnableGetBookingDetailsFromNetstormingXml, LoggerEvents.UnableGetBookingDetailsFromNetstormingXml.ToString()),
                $"WARNING | {nameof(PaymentLinkService)}: {{message}}");
            
            UnableToAcceptNetstormingRequestEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.UnableGetBookingDetailsFromNetstormingXml, LoggerEvents.UnableGetBookingDetailsFromNetstormingXml.ToString()),
                $"WARNING | {nameof(PaymentLinkService)}: {{message}}");
            
            BookingFinalizationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.BookingFinalizationFailure, LoggerEvents.BookingFinalizationFailure.ToString()),
                $"ERROR | {nameof(BookingService)}: {{message}}");
            
            BookingFinalizationPaymentFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.BookingFinalizationPaymentFailure, LoggerEvents.BookingFinalizationPaymentFailure.ToString()),
                $"ERROR | {nameof(BookingService)}: {{message}}");
            
            BookingFinalizationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.BookingFinalizationSuccess, LoggerEvents.BookingFinalizationSuccess.ToString()),
                $"INFORMATION | {nameof(BookingService)}: {{message}}");
            
            BookingProcessResponseFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.BookingResponseProcessFailure, LoggerEvents.BookingResponseProcessFailure.ToString()),
                $"ERROR | {nameof(BookingService)}: {{message}}");
            
            BookingProcessResponseSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.BookingResponseProcessSuccess, LoggerEvents.BookingResponseProcessSuccess.ToString()),
                $"ERROR | {nameof(BookingService)}: {{message}}");
            
            BookingProcessResponseStartedEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.BookingResponseProcessStarted, LoggerEvents.BookingResponseProcessStarted.ToString()),
                $"ERROR | {nameof(BookingService)}: {{message}}");
            
            BookingFinalizationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.BookingResponseProcessSuccess, LoggerEvents.BookingResponseProcessSuccess.ToString()),
                $"INFORMATION | {nameof(BookingService)}: {{message}}");

            AdministratorAuthorizationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.AdministratorAuthorizationSuccess, LoggerEvents.AdministratorAuthorizationSuccess.ToString()),
                $"DEBUG | {nameof(AdministratorPermissionsAuthorizationHandler)}: {{message}}");
                
            AdministratorAuthorizationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.AdministratorAuthorizationFailure, LoggerEvents.AdministratorAuthorizationFailure.ToString()),
                $"WARNING | {nameof(AdministratorPermissionsAuthorizationHandler)}: {{message}}");
            
            AgentAuthorizationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.AgentAuthorizationSuccess, LoggerEvents.AgentAuthorizationSuccess.ToString()),
                $"DEBUG | {nameof(InAgencyPermissionAuthorizationHandler)}: {{message}}");
                
            AgentAuthorizationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.AgentAuthorizationFailure, LoggerEvents.AgentAuthorizationFailure.ToString()),
                $"WARNING | {nameof(InAgencyPermissionAuthorizationHandler)}: {{message}}");
            
            ServiceAccountAuthorizationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.ServiceAccountAuthorizationSuccess, LoggerEvents.ServiceAccountAuthorizationSuccess.ToString()),
                $"DEBUG | {nameof(InAgencyPermissionAuthorizationHandler)}: {{message}}");
                
            ServiceAccountAuthorizationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.ServiceAccountAuthorizationFailure, LoggerEvents.ServiceAccountAuthorizationFailure.ToString()),
                $"WARNING | {nameof(InAgencyPermissionAuthorizationHandler)}: {{message}}");
            
            CounterpartyStateCheckSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.CounterpartyStateAuthorizationSuccess, LoggerEvents.CounterpartyStateAuthorizationSuccess.ToString()),
                $"DEBUG | {nameof(MinCounterpartyStateAuthorizationHandler)}: {{message}}");
                
            CounterpartyStateCheckFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.CounterpartyStateAuthorizationFailure, LoggerEvents.CounterpartyStateAuthorizationFailure.ToString()),
                $"WARNING | {nameof(MinCounterpartyStateAuthorizationHandler)}: {{message}}");
            
            LocationNormalized = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.LocationNormalized, LoggerEvents.LocationNormalized.ToString()),
                $"INFORMATION | {nameof(LocationNormalizer)}: {{message}}");
            
            DefaultLanguageKeyIsMissingInFieldOfLocationsTable = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.DefaultLanguageKeyIsMissingInFieldOfLocationsTable, LoggerEvents.DefaultLanguageKeyIsMissingInFieldOfLocationsTable.ToString()),
                $"WARNING | {nameof(LocationNormalizer)}: {{message}}");
            
            AvailabilitySearchStartedEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.MultiProviderAvailabilitySearchStarted, LoggerEvents.MultiProviderAvailabilitySearchStarted.ToString()),
                $"DEBUG | {nameof(AvailabilitySearchScheduler)}: {{message}}");
            
            AvailabilityProviderSearchStartedEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.ProviderAvailabilitySearchStarted, LoggerEvents.ProviderAvailabilitySearchStarted.ToString()),
                $"DEBUG | {nameof(AvailabilitySearchScheduler)}: {{message}}");
            
            AvailabilityProviderSearchSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.ProviderAvailabilitySearchSuccess, LoggerEvents.ProviderAvailabilitySearchSuccess.ToString()),
                $"DEBUG | {nameof(AvailabilitySearchScheduler)}: {{message}}");
            
            AvailabilityProviderSearchFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId((int) LoggerEvents.ProviderAvailabilitySearchFailure, LoggerEvents.ProviderAvailabilitySearchFailure.ToString()),
                $"ERROR | {nameof(AvailabilitySearchScheduler)}: {{message}}");
            
            AvailabilityProviderSearchExceptionOccured = LoggerMessage.Define<string>(LogLevel.Critical,
                new EventId((int) LoggerEvents.ProviderAvailabilitySearchException, LoggerEvents.ProviderAvailabilitySearchException.ToString()),
                $"EXCEPTION | {nameof(AvailabilitySearchScheduler)}: {{message}}");
        }


        internal static void LogDataProviderClientException(this ILogger logger, Exception exception) => DataProviderClientExceptionOccurred(logger, exception);
        
        internal static void LogDataProviderRequestError(this ILogger logger, string message) => DataProviderRequestErrorOccurred(logger, message, null);

        internal static void LogGeocoderException(this ILogger logger, Exception exception) => GeoCoderExceptionOccurred(logger, exception);


        internal static void LogInvitationCreatedInformation(this ILogger logger, string message) => InvitationCreatedEventOccured(logger, message, null);


        internal static void LogAgentRegistrationFailed(this ILogger logger, string message)
            => AgentRegistrationFailedEventOccured(logger, message, null);


        internal static void LogAgentRegistrationSuccess(this ILogger logger, string message)
            => AgentRegistrationSuccessEventOccured(logger, message, null);


        internal static void LogPaymentAccountCreationFailed(this ILogger logger, string message)
            => PaymentAccountCreationFailedEventOccured(logger, message, null);


        internal static void LogPaymentAccountCreationSuccess(this ILogger logger, string message)
            => PaymentAccountCreatedSuccessEventOccured(logger, message, null);


        internal static void LogEntityLockFailed(this ILogger logger, string message) => EntityLockFailedEventOccured(logger, message, null);


        internal static void LogPayfortClientException(this ILogger logger, Exception exception) => PayfortClientExceptionOccurred(logger, exception);

        internal static void LogPayfortError(this ILogger logger, string message) => PayfortErrorOccurred(logger, message, null);


        internal static void LogExternalPaymentLinkSendSuccess(this ILogger logger, string message)
            => ExternalPaymentLinkSendSuccessEventOccured(logger, message, null);


        internal static void LogExternalPaymentLinkSendFailed(this ILogger logger, string message)
            => ExternalPaymentLinkSendFailedEventOccured(logger, message, null);


        internal static void UnableCaptureWholeAmountForBooking(this ILogger logger, string message)
            => UnableCaptureWholeAmountForBookingEventOccured(logger, message, null);

        
        internal static void UnableToGetBookingDetailsFromNetstormingXml(this ILogger logger, string message)
            => UnableToGetBookingDetailsFromNetstormingXmlEventOccured(logger, message, null);
        
        internal static void LogBookingFinalizationFailed(this ILogger logger, string message)
            => BookingFinalizationFailedEventOccured(logger, message, null);
        
        internal static void LogBookingFinalizationSuccess(this ILogger logger, string message)
            => BookingFinalizationSuccessEventOccured(logger, message, null);
        
        internal static void LogBookingFinalizationFailedToPay(this ILogger logger, string message)
            => BookingFinalizationPaymentFailedEventOccured(logger, message, null);
        
        internal static void LogBookingProcessResponseFailed(this ILogger logger, string message)
            => BookingProcessResponseFailedEventOccured(logger, message, null);
        
        internal static void LogBookingProcessResponseStarted(this ILogger logger, string message)
            => BookingProcessResponseStartedEventOccured(logger, message, null);
        
        internal static void LogBookingProcessResponseSuccess(this ILogger logger, string message)
            => BookingProcessResponseSuccessEventOccured(logger, message, null);
        

        internal static void UnableToAcceptNetstormingRequest(this ILogger logger, string message)
            => UnableToAcceptNetstormingRequestEventOccured(logger, message, null);
        
        internal static void LogAdministratorAuthorized(this ILogger logger, string message)
            => AdministratorAuthorizationSuccessEventOccured(logger, message, null);
        
        internal static void LogAdministratorFailedToAuthorize(this ILogger logger, string message)
            => AdministratorAuthorizationFailedEventOccured(logger, message, null);
        
        internal static void LogAgentAuthorized(this ILogger logger, string message)
            => AgentAuthorizationSuccessEventOccured(logger, message, null);
        
        internal static void LogAgentFailedToAuthorize(this ILogger logger, string message)
            => AgentAuthorizationFailedEventOccured(logger, message, null);
        
        internal static void LogServiceAccountAuthorized(this ILogger logger, string message)
            => ServiceAccountAuthorizationSuccessEventOccured(logger, message, null);
        
        internal static void LogServiceAccountFailedToAuthorize(this ILogger logger, string message)
            => ServiceAccountAuthorizationFailedEventOccured(logger, message, null);
        
        internal static void LogCounterpartyStateChecked(this ILogger logger, string message)
            => CounterpartyStateCheckSuccessEventOccured(logger, message, null);
        
        internal static void LogCounterpartyStateCheckFailed(this ILogger logger, string message)
            => CounterpartyStateCheckFailedEventOccured(logger, message, null);
        
        internal static void LogLocationNormalized(this ILogger logger, string message)
            => LocationNormalized(logger, message, null);
        
        internal static void LogMultiProviderAvailabilitySearchStarted(this ILogger logger, string message)
            => AvailabilitySearchStartedEventOccured(logger, message, null);
        
        internal static void LogAvailabilityProviderSearchTaskStarted(this ILogger logger, string message)
            => AvailabilityProviderSearchStartedEventOccured(logger, message, null);
        
        internal static void LogAvailabilityProviderSearchTaskFinishedSuccess(this ILogger logger, string message)
            => AvailabilityProviderSearchSuccessEventOccured(logger, message, null);
        
        internal static void LogAvailabilityProviderSearchTaskFinishedError(this ILogger logger, string message)
            => AvailabilityProviderSearchFailedEventOccured(logger, message, null);
        
        internal static void LogAvailabilityProviderSearchTaskFinishedException(this ILogger logger, string message, Exception exception)
            => AvailabilityProviderSearchExceptionOccured(logger, message, exception);
        
        internal static void LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(this ILogger logger, string message)
            => DefaultLanguageKeyIsMissingInFieldOfLocationsTable(logger, message, null);
        
        
        private static readonly Action<ILogger, Exception> DataProviderClientExceptionOccurred;
        private static readonly Action<ILogger, string, Exception> DataProviderRequestErrorOccurred;
        private static readonly Action<ILogger, Exception> GeoCoderExceptionOccurred;
        private static readonly Action<ILogger, Exception> PayfortClientExceptionOccurred;
        private static readonly Action<ILogger, string, Exception> PayfortErrorOccurred;
        private static readonly Action<ILogger, string, Exception> InvitationCreatedEventOccured;
        private static readonly Action<ILogger, string, Exception> AgentRegistrationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> AgentRegistrationSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> PaymentAccountCreationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> PaymentAccountCreatedSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> EntityLockFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> ExternalPaymentLinkSendSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> ExternalPaymentLinkSendFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> UnableCaptureWholeAmountForBookingEventOccured;
        private static readonly Action<ILogger, string, Exception> UnableToGetBookingDetailsFromNetstormingXmlEventOccured;
        private static readonly Action<ILogger, string, Exception> UnableToAcceptNetstormingRequestEventOccured;

        private static readonly Action<ILogger, string, Exception> BookingFinalizationPaymentFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> BookingFinalizationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> BookingFinalizationSuccessEventOccured;
        
        private static readonly Action<ILogger, string, Exception> BookingProcessResponseFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> BookingProcessResponseSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> BookingProcessResponseStartedEventOccured;
        
        private static readonly Action<ILogger, string, Exception> AdministratorAuthorizationSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> AdministratorAuthorizationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> AgentAuthorizationSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> AgentAuthorizationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> ServiceAccountAuthorizationSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> ServiceAccountAuthorizationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> CounterpartyStateCheckSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> CounterpartyStateCheckFailedEventOccured;
        
        private static readonly Action<ILogger, string, Exception> LocationNormalized;

        private static readonly Action<ILogger, string, Exception> AvailabilitySearchStartedEventOccured;
        private static readonly Action<ILogger, string, Exception> AvailabilityProviderSearchStartedEventOccured;
        private static readonly Action<ILogger, string, Exception> AvailabilityProviderSearchSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> AvailabilityProviderSearchFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> AvailabilityProviderSearchExceptionOccured;
        
        private static readonly Action<ILogger, string, Exception> DefaultLanguageKeyIsMissingInFieldOfLocationsTable;
    }
}