using System;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CompanyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InCompanyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Customers;
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
                $"INFORMATION | {nameof(CustomerInvitationService)}: {{message}}");
            CustomerRegistrationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.CustomerRegistrationFailed, LoggerEvents.CustomerRegistrationFailed.ToString()),
                $"ERROR | {nameof(CustomerRegistrationService)}: {{message}}");
            CustomerRegistrationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId((int) LoggerEvents.CustomerRegistrationSuccess, LoggerEvents.CustomerRegistrationSuccess.ToString()),
                $"INFORMATION | {nameof(CustomerRegistrationService)}: {{message}}");
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
            
            CustomerAuthorizationSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.CustomerAuthorizationSuccess, LoggerEvents.CustomerAuthorizationSuccess.ToString()),
                $"DEBUG | {nameof(InCompanyPermissionAuthorizationHandler)}: {{message}}");
                
            CustomerAuthorizationFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.CustomerAuthorizationFailure, LoggerEvents.CustomerAuthorizationFailure.ToString()),
                $"WARNING | {nameof(InCompanyPermissionAuthorizationHandler)}: {{message}}");
            
            CompanyStateCheckSuccessEventOccured = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId((int) LoggerEvents.CompanyStateAuthorizationSuccess, LoggerEvents.CompanyStateAuthorizationSuccess.ToString()),
                $"DEBUG | {nameof(MinCompanyStateAuthorizationHandler)}: {{message}}");
                
            CompanyStateCheckFailedEventOccured = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId((int) LoggerEvents.CompanyStateAuthorizationFailure, LoggerEvents.CompanyStateAuthorizationFailure.ToString()),
                $"WARNING | {nameof(MinCompanyStateAuthorizationHandler)}: {{message}}");
        }


        internal static void LogDataProviderClientException(this ILogger logger, Exception exception) => DataProviderClientExceptionOccurred(logger, exception);
        
        internal static void LogDataProviderRequestError(this ILogger logger, string message) => DataProviderRequestErrorOccurred(logger, message, null);

        internal static void LogGeocoderException(this ILogger logger, Exception exception) => GeoCoderExceptionOccurred(logger, exception);


        internal static void LogInvitationCreatedInformation(this ILogger logger, string message) => InvitationCreatedEventOccured(logger, message, null);


        internal static void LogCustomerRegistrationFailed(this ILogger logger, string message)
            => CustomerRegistrationFailedEventOccured(logger, message, null);


        internal static void LogCustomerRegistrationSuccess(this ILogger logger, string message)
            => CustomerRegistrationSuccessEventOccured(logger, message, null);


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
        
        internal static void LogCustomerAuthorized(this ILogger logger, string message)
            => CustomerAuthorizationSuccessEventOccured(logger, message, null);
        
        internal static void LogCustomerFailedToAuthorize(this ILogger logger, string message)
            => CustomerAuthorizationFailedEventOccured(logger, message, null);
        
        internal static void LogCompanyStateChecked(this ILogger logger, string message)
            => CompanyStateCheckSuccessEventOccured(logger, message, null);
        
        internal static void LogCompanyStateCheckFailed(this ILogger logger, string message)
            => CompanyStateCheckFailedEventOccured(logger, message, null);
        
        
        private static readonly Action<ILogger, Exception> DataProviderClientExceptionOccurred;
        private static readonly Action<ILogger, string, Exception> DataProviderRequestErrorOccurred;
        private static readonly Action<ILogger, Exception> GeoCoderExceptionOccurred;
        private static readonly Action<ILogger, Exception> PayfortClientExceptionOccurred;
        private static readonly Action<ILogger, string, Exception> PayfortErrorOccurred;
        private static readonly Action<ILogger, string, Exception> InvitationCreatedEventOccured;
        private static readonly Action<ILogger, string, Exception> CustomerRegistrationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> CustomerRegistrationSuccessEventOccured;
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
        private static readonly Action<ILogger, string, Exception> CustomerAuthorizationSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> CustomerAuthorizationFailedEventOccured;
        private static readonly Action<ILogger, string, Exception> CompanyStateCheckSuccessEventOccured;
        private static readonly Action<ILogger, string, Exception> CompanyStateCheckFailedEventOccured;
    }
}