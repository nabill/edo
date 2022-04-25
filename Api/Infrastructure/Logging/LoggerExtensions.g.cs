using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            GeoCoderException = LoggerMessage.Define(LogLevel.Error,
                new EventId(1001, "GeoCoderException"),
                "Getting google response exception");
            
            InvitationCreated = LoggerMessage.Define<HappyTravel.Edo.Common.Enums.UserInvitationTypes, string>(LogLevel.Information,
                new EventId(1006, "InvitationCreated"),
                "The invitation with type {InvitationType} created for the user '{Email}'");
            
            AgentRegistrationFailed = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1007, "AgentRegistrationFailed"),
                "Agent registration failed with error `{Error}`");
            
            AgentRegistrationSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1008, "AgentRegistrationSuccess"),
                "Agent {Email} successfully registered");
            
            PayfortClientException = LoggerMessage.Define(LogLevel.Critical,
                new EventId(1009, "PayfortClientException"),
                "Payfort client exception");
            
            AgencyAccountCreationSuccess = LoggerMessage.Define<int, int>(LogLevel.Information,
                new EventId(1010, "AgencyAccountCreationSuccess"),
                "Successfully created account for agency: '{AgencyId}', account id: {AccountId}");
            
            AgencyAccountCreationFailed = LoggerMessage.Define<int, string>(LogLevel.Error,
                new EventId(1011, "AgencyAccountCreationFailed"),
                "Failed to create account for agency {AgencyId}, error {Error}");
            
            EntityLockFailed = LoggerMessage.Define<string, string>(LogLevel.Critical,
                new EventId(1012, "EntityLockFailed"),
                "Failed to lock entity {EntityType} with id: {EntityId}");
            
            PayfortError = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1013, "PayfortError"),
                "Error deserializing payfort response: '{Content}'");
            
            ExternalPaymentLinkSendSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1014, "ExternalPaymentLinkSendSuccess"),
                "Successfully sent e-mail to {Email}");
            
            ExternalPaymentLinkSendFailed = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1015, "ExternalPaymentLinkSendFailed"),
                "Error sending email to {Email}: {Error}");
            
            UnableGetBookingDetailsFromNetstormingXml = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1017, "UnableGetBookingDetailsFromNetstormingXml"),
                "Failed to get booking details from the Netstorming xml: {Xml}");
            
            UnableToAcceptNetstormingRequest = LoggerMessage.Define(LogLevel.Warning,
                new EventId(1018, "UnableToAcceptNetstormingRequest"),
                "Unable to accept netstorming request");
            
            BookingFinalizationFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1020, "BookingFinalizationFailure"),
                "The booking finalization with the reference code: '{ReferenceCode}' has been failed with a message: {Message}");
            
            BookingFinalizationPaymentFailure = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1021, "BookingFinalizationPaymentFailure"),
                "The booking with reference code: '{ReferenceCode}' hasn't been paid");
            
            BookingFinalizationSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1022, "BookingFinalizationSuccess"),
                "The booking finalization successfully completed. Reference code: '{ReferenceCode}'");
            
            BookingFinalizationException = LoggerMessage.Define(LogLevel.Critical,
                new EventId(1023, "BookingFinalizationException"),
                "Booking finalization exception");
            
            BookingResponseProcessFailure = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1030, "BookingResponseProcessFailure"),
                "Booking response process failure. Error: {Error}");
            
            BookingResponseProcessSuccess = LoggerMessage.Define<string, string>(LogLevel.Information,
                new EventId(1031, "BookingResponseProcessSuccess"),
                "The booking response with the reference code '{ReferenceCode} has been processed with message {Message}");
            
            BookingResponseProcessStarted = LoggerMessage.Define<string, HappyTravel.Edo.Common.Enums.BookingStatuses>(LogLevel.Information,
                new EventId(1032, "BookingResponseProcessStarted"),
                "Start the booking response processing with the reference code '{ReferenceCode}'. Old status: {Status}");
            
            BookingCancelFailure = LoggerMessage.Define<string, string>(LogLevel.Critical,
                new EventId(1040, "BookingCancelFailure"),
                "Failed to cancel a booking with reference code: '{ReferenceCode}'. Error: {Error}");
            
            BookingCancelSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1041, "BookingCancelSuccess"),
                "Successfully cancelled a booking with reference code: '{ReferenceCode}'");
            
            BookingAlreadyCancelled = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1042, "BookingAlreadyCancelled"),
                "Skipping cancellation for a booking with reference code: '{ReferenceCode}'. Already cancelled.");
            
            BookingRegistrationSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1050, "BookingRegistrationSuccess"),
                "Successfully registered a booking with reference code: '{ReferenceCode}");
            
            BookingRegistrationFailure = LoggerMessage.Define<string, string, string, string>(LogLevel.Error,
                new EventId(1051, "BookingRegistrationFailure"),
                "Failed to register a booking. HtId: {HtId}, Itinerary number: {ItineraryNumber}. Passenger name: {MainPassengerName}. Error: {Error}");
            
            BookingByAccountSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1060, "BookingByAccountSuccess"),
                "Successfully booked using account. Reference code: '{ReferenceCode}'");
            
            BookingByAccountFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1061, "BookingByAccountFailure"),
                "Failed to book using account. HtId: '{HtId}'. Error: {Error}");
            
            BookingByAccountStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1062, "BookingByAccountStarted"),
                "Book using account started. HtId: '{HtId}'");
            
            BookingByOfflinePaymentSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1063, "BookingByOfflinePaymentSuccess"),
                "Successfully booked using offline payment. Reference code: '{ReferenceCode}'}");
            
            BookingByOfflinePaymentFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1064, "BookingByOfflinePaymentFailure"),
                "Failed to book using offline payment. HtId: '{HtId}'. Error: {Error}");
            
            BookingByOfflinePaymentStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1065, "BookingByOfflinePaymentStarted"),
                "Book using offline payment started. HtId: '{HtId}'");
            
            BookingRefreshStatusSuccess = LoggerMessage.Define<string, HappyTravel.Edo.Common.Enums.BookingStatuses, HappyTravel.EdoContracts.Accommodations.Enums.BookingStatusCodes>(LogLevel.Information,
                new EventId(1070, "BookingRefreshStatusSuccess"),
                "Successfully refreshed status for a booking with reference code: '{ReferenceCode}'. Old status: {OldStatus}. New status: {Status}");
            
            BookingRefreshStatusFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1071, "BookingRefreshStatusFailure"),
                "Failed to refresh status for a booking with reference code: '{ReferenceCode}' while getting info from a supplier. Error: {Error}");
            
            BookingConfirmationFailure = LoggerMessage.Define<string, string>(LogLevel.Critical,
                new EventId(1072, "BookingConfirmationFailure"),
                "Booking '{ReferenceCode} confirmation failed: '{Error}");
            
            BookingEvaluationFailure = LoggerMessage.Define<System.Nullable<int>, string>(LogLevel.Warning,
                new EventId(1073, "BookingEvaluationFailure"),
                "EvaluateOnConnector returned status code: {Status}, error: {Error}");
            
            BookingEvaluationCancellationPoliciesFailure = LoggerMessage.Define(LogLevel.Error,
                new EventId(1074, "BookingEvaluationCancellationPoliciesFailure"),
                "EvaluateOnConnector returned cancellation policies with 0 penalty");
            
            ExternalAdministratorAuthorizationSuccess = LoggerMessage.Define(LogLevel.Debug,
                new EventId(1100, "ExternalAdministratorAuthorizationSuccess"),
                "Successfully authorized external administrator");
            
            AdministratorAuthorizationFailure = LoggerMessage.Define(LogLevel.Warning,
                new EventId(1101, "AdministratorAuthorizationFailure"),
                "Administrator authorization failed");
            
            InternalAdministratorAuthorizationSuccess = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1103, "InternalAdministratorAuthorizationSuccess"),
                "Successfully authorized administrator '{Email}'");
            
            AgentAuthorizationSuccess = LoggerMessage.Define<string, string>(LogLevel.Debug,
                new EventId(1110, "AgentAuthorizationSuccess"),
                "Successfully authorized agent '{Email}' for '{Permissions}'");
            
            AgentAuthorizationFailure = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1111, "AgentAuthorizationFailure"),
                "Agent authorization failure: '{Error}'");
            
            ServiceAccountAuthorizationSuccess = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1125, "ServiceAccountAuthorizationSuccess"),
                "Service account '{ClientId}' authorized successfully");
            
            ServiceAccountAuthorizationFailure = LoggerMessage.Define<string>(LogLevel.Warning,
                new EventId(1126, "ServiceAccountAuthorizationFailure"),
                "Service account authorization failed. Error: {Error}");
            
            LocationNormalized = LoggerMessage.Define(LogLevel.Information,
                new EventId(1130, "LocationNormalized"),
                "Location normalized");
            
            MultiSupplierAvailabilitySearchStarted = LoggerMessage.Define<string, string, string[], string, int>(LogLevel.Information,
                new EventId(1140, "MultiSupplierAvailabilitySearchStarted"),
                "Starting availability search for {CheckInDate} - {CheckOutDate}. Locations: '{LocationHtIds}', nationality: '{Nationality}', rooms: {RoomCount}");
            
            SupplierAvailabilitySearchStarted = LoggerMessage.Define<System.Guid, string>(LogLevel.Information,
                new EventId(1141, "SupplierAvailabilitySearchStarted"),
                "Availability search with id '{SearchId}' on supplier '{Supplier}' started");
            
            SupplierAvailabilitySearchSuccess = LoggerMessage.Define<System.Guid, string, int>(LogLevel.Information,
                new EventId(1142, "SupplierAvailabilitySearchSuccess"),
                "Availability search with id '{SearchId}' on supplier '{Supplier}' finished successfully with '{ResultCount}' results");
            
            SupplierAvailabilitySearchFailure = LoggerMessage.Define<System.Guid, string, HappyTravel.Edo.Api.Models.Availabilities.AvailabilitySearchTaskState, string>(LogLevel.Warning,
                new EventId(1143, "SupplierAvailabilitySearchFailure"),
                "Availability search with id '{SearchId}' on supplier '{Supplier}' finished with state '{TaskState}', error '{Error}'");
            
            SupplierAvailabilitySearchException = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1145, "SupplierAvailabilitySearchException"),
                "Supplier availability search exception on supplier '{Supplier}'");
            
            AgencyVerificationStateAuthorizationSuccess = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1150, "AgencyVerificationStateAuthorizationSuccess"),
                "Successfully checked agency verification state for agent {Email}");
            
            AgencyVerificationStateAuthorizationFailure = LoggerMessage.Define<string, HappyTravel.Edo.Common.Enums.AgencyVerificationStates>(LogLevel.Warning,
                new EventId(1151, "AgencyVerificationStateAuthorizationFailure"),
                "Agency of agent '{Email}' has wrong verification state {State}");
            
            DefaultLanguageKeyIsMissingInFieldOfLocationsTable = LoggerMessage.Define(LogLevel.Warning,
                new EventId(1200, "DefaultLanguageKeyIsMissingInFieldOfLocationsTable"),
                "Default language key is missing in field of locations table");
            
            ConnectorClientException = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1300, "ConnectorClientException"),
                "Connector client exception, url {RequestUrl}, response: {Response}");
            
            SupplierConnectorRequestError = LoggerMessage.Define<string, string, string, System.Nullable<int>>(LogLevel.Error,
                new EventId(1301, "SupplierConnectorRequestError"),
                "Error executing connector request to {Url}: '{Error}', operation: {OperationName}, status code: '{Status}'");
            
            SupplierConnectorRequestSuccess = LoggerMessage.Define<string, string>(LogLevel.Information,
                new EventId(1302, "SupplierConnectorRequestSuccess"),
                "completed executing connector request to {Url}, operation {OperationName}");
            
            SupplierConnectorRequestStarted = LoggerMessage.Define<string, string>(LogLevel.Information,
                new EventId(1303, "SupplierConnectorRequestStarted"),
                "Started executing connector request to {Url}, operation {OperationName}");
            
            GetTokenForConnectorError = LoggerMessage.Define<string, string, System.DateTime>(LogLevel.Error,
                new EventId(1310, "GetTokenForConnectorError"),
                "Something went wrong while requesting the access token. Error: {Error}. Using existing token: '{Token}' with expiry date '{ExpiryDate}'");
            
            UnauthorizedConnectorResponse = LoggerMessage.Define<string>(LogLevel.Debug,
                new EventId(1311, "UnauthorizedConnectorResponse"),
                "Unauthorized response was returned from '{RequestUri}'. Refreshing token...");
            
            CaptureMoneyForBookingSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1400, "CaptureMoneyForBookingSuccess"),
                "Successfully captured money for a booking with reference code: '{ReferenceCode}'");
            
            CaptureMoneyForBookingFailure = LoggerMessage.Define<string, HappyTravel.Edo.Common.Enums.PaymentTypes>(LogLevel.Error,
                new EventId(1401, "CaptureMoneyForBookingFailure"),
                "Failed to capture money for a booking with reference code: '{ReferenceCode}'. Error: Invalid payment method: {PaymentType}");
            
            ChargeMoneyForBookingSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1402, "ChargeMoneyForBookingSuccess"),
                "Successfully charged money for a booking with reference code: '{ReferenceCode}'");
            
            ChargeMoneyForBookingFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1403, "ChargeMoneyForBookingFailure"),
                "Failed to charge money for a booking with reference code: '{ReferenceCode} with error {Error}");
            
            ProcessPaymentChangesForBookingSuccess = LoggerMessage.Define<HappyTravel.Edo.Common.Enums.BookingPaymentStatuses, HappyTravel.Edo.Common.Enums.PaymentStatuses, string, string>(LogLevel.Information,
                new EventId(1410, "ProcessPaymentChangesForBookingSuccess"),
                "Successfully processes payment changes. Old payment status: {OldPaymentStatus}. New payment status: {PaymentStatus}. Payment: '{PaymentReferenceCode}'. Booking reference code: '{BookingReferenceCode}'");
            
            ProcessPaymentChangesForBookingSkip = LoggerMessage.Define<HappyTravel.Edo.Common.Enums.PaymentStatuses, string, string>(LogLevel.Warning,
                new EventId(1411, "ProcessPaymentChangesForBookingSkip"),
                "Skipped booking status update while processing payment changes. Payment status: {PaymentStatus}. Payment: '{PaymentReferenceCode}'. Booking reference code: '{BookingReferenceCode}'");
            
            ProcessPaymentChangesForBookingFailure = LoggerMessage.Define<HappyTravel.Edo.Common.Enums.PaymentStatuses, string>(LogLevel.Error,
                new EventId(1412, "ProcessPaymentChangesForBookingFailure"),
                "Failed to process payment changes, could not find the corresponding booking. Payment status: {Status}. Payment: '{ReferenceCode}'");
            
            ElasticAnalyticsEventSendError = LoggerMessage.Define(LogLevel.Error,
                new EventId(1501, "ElasticAnalyticsEventSendError"),
                "Sending event to elasticsearch failed");
            
            MapperClientException = LoggerMessage.Define(LogLevel.Error,
                new EventId(1601, "MapperClientException"),
                "Mapper client exception");
            
            MapperClientErrorResponse = LoggerMessage.Define<string, int, string[]>(LogLevel.Error,
                new EventId(1602, "MapperClientErrorResponse"),
                "Request to mapper failed: {Message}:{StatusCode}. Requested HtIds {HtIds}");
            
            MapperManagementClientException = LoggerMessage.Define(LogLevel.Error,
                new EventId(1603, "MapperManagementClientException"),
                "Mapper management client exception");
            
            AgencyAccountAddedNotificationFailure = LoggerMessage.Define<int, string>(LogLevel.Error,
                new EventId(1701, "AgencyAccountAddedNotificationFailure"),
                "Agency {AgencyId} account added notification failed with error {Error}");
            
            AgentRegistrationNotificationFailure = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1702, "AgentRegistrationNotificationFailure"),
                "Agent registration notification failure with error {Error}");
            
            ChildAgencyRegistrationNotificationFailure = LoggerMessage.Define(LogLevel.Error,
                new EventId(1703, "ChildAgencyRegistrationNotificationFailure"),
                "Child agency registration notification failed");
            
            AgencyAccountSubtractedNotificationFailure = LoggerMessage.Define<int, string>(LogLevel.Error,
                new EventId(1704, "AgencyAccountSubtractedNotificationFailure"),
                "Agency {AgencyId} account subtracted notification failed with error {Error}");
            
            AgencyAccountIncreasedManuallyNotificationFailure = LoggerMessage.Define<int, string>(LogLevel.Error,
                new EventId(1705, "AgencyAccountIncreasedManuallyNotificationFailure"),
                "Agency {AgencyId} account increasedManually notification failed with error {Error}");
            
            AgencyAccountDecreasedManuallyNotificationFailure = LoggerMessage.Define<int, string>(LogLevel.Error,
                new EventId(1706, "AgencyAccountDecreasedManuallyNotificationFailure"),
                "Agency {AgencyId} account decreasedManually notification failed with error {Error}");
            
            ExternalPaymentLinkGenerationSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1707, "ExternalPaymentLinkGenerationSuccess"),
                "Successfully generated payment link for {Email}");
            
            ExternalPaymentLinkGenerationFailed = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1708, "ExternalPaymentLinkGenerationFailed"),
                "Error generating payment link for {Email}: {Error}");
            
            GetAccommodationByHtIdFailed = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1709, "GetAccommodationByHtIdFailed"),
                "Error getting accommodation for HtId '{HtId}': error: {Error}");
            
            SendConfirmationEmailFailure = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1800, "SendConfirmationEmailFailure"),
                "Error sending booking confirmation email to property owner. Received empty list of email addresses from mapper. Reference code {ReferenceCode}");
            
            ConnectorClientUnexpectedResponse = LoggerMessage.Define<System.Net.HttpStatusCode, System.Uri, string>(LogLevel.Error,
                new EventId(1801, "ConnectorClientUnexpectedResponse"),
                "Unexpected response received from connector. StatusCode: `{StatusCode}`, request uri: `{Uri}`, response: {Response}");
            
            MapperClientUnexpectedResponse = LoggerMessage.Define<System.Net.HttpStatusCode, System.Uri, string>(LogLevel.Error,
                new EventId(1802, "MapperClientUnexpectedResponse"),
                "Unexpected response received from mapper. StatusCode: `{StatusCode}`, request uri: `{Uri}`, response: {Response}");
            
            MapperClientRequestTimeout = LoggerMessage.Define(LogLevel.Warning,
                new EventId(1803, "MapperClientRequestTimeout"),
                "Request to mapper failed with timeout");
            
            MapperManagementClientUnexpectedResponse = LoggerMessage.Define<System.Net.HttpStatusCode, System.Uri, string>(LogLevel.Error,
                new EventId(1804, "MapperManagementClientUnexpectedResponse"),
                "Unexpected response received from a mapper management endpoint. StatusCode: `{StatusCode}`, request uri: `{Uri}`, response: {Response}");
            
            MapperManagementClientRequestTimeout = LoggerMessage.Define(LogLevel.Warning,
                new EventId(1806, "MapperManagementClientRequestTimeout"),
                "Request to a mapper management endpoint failed with timeout");
            
            MarkupPolicyStorageRefreshed = LoggerMessage.Define<int>(LogLevel.Debug,
                new EventId(1090, "MarkupPolicyStorageRefreshed"),
                "MarkupPolicyStorage refreshed. Was set {Count} entities");
            
            MarkupPolicyStorageUpdateCompleted = LoggerMessage.Define(LogLevel.Debug,
                new EventId(1091, "MarkupPolicyStorageUpdateCompleted"),
                "Markup policy storage update completed");
            
            MarkupPolicyStorageUpdateFailed = LoggerMessage.Define(LogLevel.Error,
                new EventId(1092, "MarkupPolicyStorageUpdateFailed"),
                "Markup policy storage update failed");
            
            CurrencyConversionFailed = LoggerMessage.Define<HappyTravel.Money.Enums.Currencies, HappyTravel.Money.Enums.Currencies, string>(LogLevel.Error,
                new EventId(1093, "CurrencyConversionFailed"),
                "Currency conversion failed. Source currency: `{Source}`, target currency: `{Target}`. Error: `{Error}`");
            
            NGeniusWebhookProcessingStarted = LoggerMessage.Define(LogLevel.Information,
                new EventId(1095, "NGeniusWebhookProcessingStarted"),
                "NGenius webhook processing started");
            
            NGeniusWebhookPaymentUpdate = LoggerMessage.Define(LogLevel.Information,
                new EventId(1096, "NGeniusWebhookPaymentUpdate"),
                "Started updating payment by NGenius webhook");
            
            NGeniusWebhookPaymentLinkUpdate = LoggerMessage.Define(LogLevel.Information,
                new EventId(1097, "NGeniusWebhookPaymentLinkUpdate"),
                "Started updating payment link by NGenius webhook");
            
            BookingExceededTimeLimit = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1098, "BookingExceededTimeLimit"),
                "Booking {ReferenceCode} exceeded time limit");
            
            InvoiceGenerated = LoggerMessage.Define<string, string>(LogLevel.Information,
                new EventId(1100, "InvoiceGenerated"),
                "Generated invoice number {InvoiceNumber} for booking {ReferenceCode}");
            
            CreditCardBookingFlowStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1103, "CreditCardBookingFlowStarted"),
                "Сredit card booking flow started for htId {HtId}");
            
            VccIssueStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1104, "VccIssueStarted"),
                "Vcc issue started for booking {ReferenceCode}");
            
            CreditCardAuthorizationStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1110, "CreditCardAuthorizationStarted"),
                "Credit card authorization started. ReferenceCode: {ReferenceCode}");
            
            CreditCardAuthorizationSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1111, "CreditCardAuthorizationSuccess"),
                "Credit card authorization success. ReferenceCode: {ReferenceCode}");
            
            CreditCardAuthorizationFailure = LoggerMessage.Define<string, string>(LogLevel.Information,
                new EventId(1112, "CreditCardAuthorizationFailure"),
                "Credit card authorization failed. ReferenceCode: {ReferenceCode}, Error: {Error}");
            
            CreditCardCapturingStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1113, "CreditCardCapturingStarted"),
                "Credit card capturing started. ReferenceCode: {ReferenceCode}");
            
            CreditCardCapturingSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1114, "CreditCardCapturingSuccess"),
                "Credit card capturing success. ReferenceCode: {ReferenceCode}");
            
            CreditCardCapturingFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1115, "CreditCardCapturingFailure"),
                "Credit card capturing failed. ReferenceCode: {ReferenceCode}, Error: {Error}");
            
            CreditCardVoidingStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1116, "CreditCardVoidingStarted"),
                "Credit card voiding started. ReferenceCode: {ReferenceCode}");
            
            CreditCardVoidingSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1117, "CreditCardVoidingSuccess"),
                "Credit card voiding success. ReferenceCode: {ReferenceCode}");
            
            CreditCardVoidingFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1118, "CreditCardVoidingFailure"),
                "Credit card voiding failed. ReferenceCode: {ReferenceCode}, Error: {Error}");
            
            CreditCardRefundingStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1119, "CreditCardRefundingStarted"),
                "Credit card refunding started. ReferenceCode: {ReferenceCode}");
            
            CreditCardRefundingSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(1120, "CreditCardRefundingSuccess"),
                "Credit card refunding success. ReferenceCode: {ReferenceCode}");
            
            CreditCardRefundingFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(1121, "CreditCardRefundingFailure"),
                "Credit card refunding failed. ReferenceCode: {ReferenceCode}, Error: {Error}");
            
            CreditCardProcessingPaymentStarted = LoggerMessage.Define(LogLevel.Information,
                new EventId(1122, "CreditCardProcessingPaymentStarted"),
                "Credit card processing payment started");
            
            CreditCardProcessingPaymentSuccess = LoggerMessage.Define(LogLevel.Information,
                new EventId(1123, "CreditCardProcessingPaymentSuccess"),
                "Credit card processing payment success");
            
            CreditCardProcessingPaymentFailure = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(1124, "CreditCardProcessingPaymentFailure"),
                "Credit card processing payment failed. Error: {Error}");
            
            DiscountStorageRefreshed = LoggerMessage.Define<int>(LogLevel.Debug,
                new EventId(1130, "DiscountStorageRefreshed"),
                "Discount storage refreshed. Was set {Count} entities");
            
            DiscountStorageUpdateCompleted = LoggerMessage.Define(LogLevel.Debug,
                new EventId(1131, "DiscountStorageUpdateCompleted"),
                "Discount storage update completed");
            
            DiscountStorageUpdateFailed = LoggerMessage.Define(LogLevel.Error,
                new EventId(1132, "DiscountStorageUpdateFailed"),
                "Discount storage update failed");
            
            MarkupPoliciesSumLessThanZero = LoggerMessage.Define<int, decimal, string>(LogLevel.Warning,
                new EventId(1133, "MarkupPoliciesSumLessThanZero"),
                "Applyed markup policies' sum less than zero. AgentId: {AgentId}; Total percentage: {TotalPercentage}; Markup policies: {Policies}");
            
            TotalDeadlineShiftIsPositive = LoggerMessage.Define<int, int, int, int, int>(LogLevel.Warning,
                new EventId(1134, "TotalDeadlineShiftIsPositive"),
                "Total deadline shift is positive. AgentId: {AgentId}; AgencyId: {AgencyId}; RootShift: {RootShift}; AgencyShift: {AgencyShift}; AgentShift: {AgentShift};");
            
        }
    
                
         public static void LogGeoCoderException(this ILogger logger, Exception exception = null)
            => GeoCoderException(logger, exception);
                
         public static void LogInvitationCreated(this ILogger logger, HappyTravel.Edo.Common.Enums.UserInvitationTypes InvitationType, string Email, Exception exception = null)
            => InvitationCreated(logger, InvitationType, Email, exception);
                
         public static void LogAgentRegistrationFailed(this ILogger logger, string Error, Exception exception = null)
            => AgentRegistrationFailed(logger, Error, exception);
                
         public static void LogAgentRegistrationSuccess(this ILogger logger, string Email, Exception exception = null)
            => AgentRegistrationSuccess(logger, Email, exception);
                
         public static void LogPayfortClientException(this ILogger logger, Exception exception = null)
            => PayfortClientException(logger, exception);
                
         public static void LogAgencyAccountCreationSuccess(this ILogger logger, int AgencyId, int AccountId, Exception exception = null)
            => AgencyAccountCreationSuccess(logger, AgencyId, AccountId, exception);
                
         public static void LogAgencyAccountCreationFailed(this ILogger logger, int AgencyId, string Error, Exception exception = null)
            => AgencyAccountCreationFailed(logger, AgencyId, Error, exception);
                
         public static void LogEntityLockFailed(this ILogger logger, string EntityType, string EntityId, Exception exception = null)
            => EntityLockFailed(logger, EntityType, EntityId, exception);
                
         public static void LogPayfortError(this ILogger logger, string Content, Exception exception = null)
            => PayfortError(logger, Content, exception);
                
         public static void LogExternalPaymentLinkSendSuccess(this ILogger logger, string Email, Exception exception = null)
            => ExternalPaymentLinkSendSuccess(logger, Email, exception);
                
         public static void LogExternalPaymentLinkSendFailed(this ILogger logger, string Email, string Error, Exception exception = null)
            => ExternalPaymentLinkSendFailed(logger, Email, Error, exception);
                
         public static void LogUnableGetBookingDetailsFromNetstormingXml(this ILogger logger, string Xml, Exception exception = null)
            => UnableGetBookingDetailsFromNetstormingXml(logger, Xml, exception);
                
         public static void LogUnableToAcceptNetstormingRequest(this ILogger logger, Exception exception = null)
            => UnableToAcceptNetstormingRequest(logger, exception);
                
         public static void LogBookingFinalizationFailure(this ILogger logger, string ReferenceCode, string Message, Exception exception = null)
            => BookingFinalizationFailure(logger, ReferenceCode, Message, exception);
                
         public static void LogBookingFinalizationPaymentFailure(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingFinalizationPaymentFailure(logger, ReferenceCode, exception);
                
         public static void LogBookingFinalizationSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingFinalizationSuccess(logger, ReferenceCode, exception);
                
         public static void LogBookingFinalizationException(this ILogger logger, Exception exception = null)
            => BookingFinalizationException(logger, exception);
                
         public static void LogBookingResponseProcessFailure(this ILogger logger, string Error, Exception exception = null)
            => BookingResponseProcessFailure(logger, Error, exception);
                
         public static void LogBookingResponseProcessSuccess(this ILogger logger, string ReferenceCode, string Message, Exception exception = null)
            => BookingResponseProcessSuccess(logger, ReferenceCode, Message, exception);
                
         public static void LogBookingResponseProcessStarted(this ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.BookingStatuses Status, Exception exception = null)
            => BookingResponseProcessStarted(logger, ReferenceCode, Status, exception);
                
         public static void LogBookingCancelFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => BookingCancelFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogBookingCancelSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingCancelSuccess(logger, ReferenceCode, exception);
                
         public static void LogBookingAlreadyCancelled(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingAlreadyCancelled(logger, ReferenceCode, exception);
                
         public static void LogBookingRegistrationSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingRegistrationSuccess(logger, ReferenceCode, exception);
                
         public static void LogBookingRegistrationFailure(this ILogger logger, string HtId, string ItineraryNumber, string MainPassengerName, string Error, Exception exception = null)
            => BookingRegistrationFailure(logger, HtId, ItineraryNumber, MainPassengerName, Error, exception);
                
         public static void LogBookingByAccountSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingByAccountSuccess(logger, ReferenceCode, exception);
                
         public static void LogBookingByAccountFailure(this ILogger logger, string HtId, string Error, Exception exception = null)
            => BookingByAccountFailure(logger, HtId, Error, exception);
                
         public static void LogBookingByAccountStarted(this ILogger logger, string HtId, Exception exception = null)
            => BookingByAccountStarted(logger, HtId, exception);
                
         public static void LogBookingByOfflinePaymentSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingByOfflinePaymentSuccess(logger, ReferenceCode, exception);
                
         public static void LogBookingByOfflinePaymentFailure(this ILogger logger, string HtId, string Error, Exception exception = null)
            => BookingByOfflinePaymentFailure(logger, HtId, Error, exception);
                
         public static void LogBookingByOfflinePaymentStarted(this ILogger logger, string HtId, Exception exception = null)
            => BookingByOfflinePaymentStarted(logger, HtId, exception);
                
         public static void LogBookingRefreshStatusSuccess(this ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.BookingStatuses OldStatus, HappyTravel.EdoContracts.Accommodations.Enums.BookingStatusCodes Status, Exception exception = null)
            => BookingRefreshStatusSuccess(logger, ReferenceCode, OldStatus, Status, exception);
                
         public static void LogBookingRefreshStatusFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => BookingRefreshStatusFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogBookingConfirmationFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => BookingConfirmationFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogBookingEvaluationFailure(this ILogger logger, System.Nullable<int> Status, string Error, Exception exception = null)
            => BookingEvaluationFailure(logger, Status, Error, exception);
                
         public static void LogBookingEvaluationCancellationPoliciesFailure(this ILogger logger, Exception exception = null)
            => BookingEvaluationCancellationPoliciesFailure(logger, exception);
                
         public static void LogExternalAdministratorAuthorizationSuccess(this ILogger logger, Exception exception = null)
            => ExternalAdministratorAuthorizationSuccess(logger, exception);
                
         public static void LogAdministratorAuthorizationFailure(this ILogger logger, Exception exception = null)
            => AdministratorAuthorizationFailure(logger, exception);
                
         public static void LogInternalAdministratorAuthorizationSuccess(this ILogger logger, string Email, Exception exception = null)
            => InternalAdministratorAuthorizationSuccess(logger, Email, exception);
                
         public static void LogAgentAuthorizationSuccess(this ILogger logger, string Email, string Permissions, Exception exception = null)
            => AgentAuthorizationSuccess(logger, Email, Permissions, exception);
                
         public static void LogAgentAuthorizationFailure(this ILogger logger, string Error, Exception exception = null)
            => AgentAuthorizationFailure(logger, Error, exception);
                
         public static void LogServiceAccountAuthorizationSuccess(this ILogger logger, string ClientId, Exception exception = null)
            => ServiceAccountAuthorizationSuccess(logger, ClientId, exception);
                
         public static void LogServiceAccountAuthorizationFailure(this ILogger logger, string Error, Exception exception = null)
            => ServiceAccountAuthorizationFailure(logger, Error, exception);
                
         public static void LogLocationNormalized(this ILogger logger, Exception exception = null)
            => LocationNormalized(logger, exception);
                
         public static void LogMultiSupplierAvailabilitySearchStarted(this ILogger logger, string CheckInDate, string CheckOutDate, string[] LocationHtIds, string Nationality, int RoomCount, Exception exception = null)
            => MultiSupplierAvailabilitySearchStarted(logger, CheckInDate, CheckOutDate, LocationHtIds, Nationality, RoomCount, exception);
                
         public static void LogSupplierAvailabilitySearchStarted(this ILogger logger, System.Guid SearchId, string Supplier, Exception exception = null)
            => SupplierAvailabilitySearchStarted(logger, SearchId, Supplier, exception);
                
         public static void LogSupplierAvailabilitySearchSuccess(this ILogger logger, System.Guid SearchId, string Supplier, int ResultCount, Exception exception = null)
            => SupplierAvailabilitySearchSuccess(logger, SearchId, Supplier, ResultCount, exception);
                
         public static void LogSupplierAvailabilitySearchFailure(this ILogger logger, System.Guid SearchId, string Supplier, HappyTravel.Edo.Api.Models.Availabilities.AvailabilitySearchTaskState TaskState, string Error, Exception exception = null)
            => SupplierAvailabilitySearchFailure(logger, SearchId, Supplier, TaskState, Error, exception);
                
         public static void LogSupplierAvailabilitySearchException(this ILogger logger, string Supplier, Exception exception = null)
            => SupplierAvailabilitySearchException(logger, Supplier, exception);
                
         public static void LogAgencyVerificationStateAuthorizationSuccess(this ILogger logger, string Email, Exception exception = null)
            => AgencyVerificationStateAuthorizationSuccess(logger, Email, exception);
                
         public static void LogAgencyVerificationStateAuthorizationFailure(this ILogger logger, string Email, HappyTravel.Edo.Common.Enums.AgencyVerificationStates State, Exception exception = null)
            => AgencyVerificationStateAuthorizationFailure(logger, Email, State, exception);
                
         public static void LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(this ILogger logger, Exception exception = null)
            => DefaultLanguageKeyIsMissingInFieldOfLocationsTable(logger, exception);
                
         public static void LogConnectorClientException(this ILogger logger, string RequestUrl, string Response, Exception exception = null)
            => ConnectorClientException(logger, RequestUrl, Response, exception);
                
         public static void LogSupplierConnectorRequestError(this ILogger logger, string Url, string Error, string OperationName, System.Nullable<int> Status, Exception exception = null)
            => SupplierConnectorRequestError(logger, Url, Error, OperationName, Status, exception);
                
         public static void LogSupplierConnectorRequestSuccess(this ILogger logger, string Url, string OperationName, Exception exception = null)
            => SupplierConnectorRequestSuccess(logger, Url, OperationName, exception);
                
         public static void LogSupplierConnectorRequestStarted(this ILogger logger, string Url, string OperationName, Exception exception = null)
            => SupplierConnectorRequestStarted(logger, Url, OperationName, exception);
                
         public static void LogGetTokenForConnectorError(this ILogger logger, string Error, string Token, System.DateTime ExpiryDate, Exception exception = null)
            => GetTokenForConnectorError(logger, Error, Token, ExpiryDate, exception);
                
         public static void LogUnauthorizedConnectorResponse(this ILogger logger, string RequestUri, Exception exception = null)
            => UnauthorizedConnectorResponse(logger, RequestUri, exception);
                
         public static void LogCaptureMoneyForBookingSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CaptureMoneyForBookingSuccess(logger, ReferenceCode, exception);
                
         public static void LogCaptureMoneyForBookingFailure(this ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.PaymentTypes PaymentType, Exception exception = null)
            => CaptureMoneyForBookingFailure(logger, ReferenceCode, PaymentType, exception);
                
         public static void LogChargeMoneyForBookingSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => ChargeMoneyForBookingSuccess(logger, ReferenceCode, exception);
                
         public static void LogChargeMoneyForBookingFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => ChargeMoneyForBookingFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogProcessPaymentChangesForBookingSuccess(this ILogger logger, HappyTravel.Edo.Common.Enums.BookingPaymentStatuses OldPaymentStatus, HappyTravel.Edo.Common.Enums.PaymentStatuses PaymentStatus, string PaymentReferenceCode, string BookingReferenceCode, Exception exception = null)
            => ProcessPaymentChangesForBookingSuccess(logger, OldPaymentStatus, PaymentStatus, PaymentReferenceCode, BookingReferenceCode, exception);
                
         public static void LogProcessPaymentChangesForBookingSkip(this ILogger logger, HappyTravel.Edo.Common.Enums.PaymentStatuses PaymentStatus, string PaymentReferenceCode, string BookingReferenceCode, Exception exception = null)
            => ProcessPaymentChangesForBookingSkip(logger, PaymentStatus, PaymentReferenceCode, BookingReferenceCode, exception);
                
         public static void LogProcessPaymentChangesForBookingFailure(this ILogger logger, HappyTravel.Edo.Common.Enums.PaymentStatuses Status, string ReferenceCode, Exception exception = null)
            => ProcessPaymentChangesForBookingFailure(logger, Status, ReferenceCode, exception);
                
         public static void LogElasticAnalyticsEventSendError(this ILogger logger, Exception exception = null)
            => ElasticAnalyticsEventSendError(logger, exception);
                
         public static void LogMapperClientException(this ILogger logger, Exception exception = null)
            => MapperClientException(logger, exception);
                
         public static void LogMapperClientErrorResponse(this ILogger logger, string Message, int StatusCode, string[] HtIds, Exception exception = null)
            => MapperClientErrorResponse(logger, Message, StatusCode, HtIds, exception);
                
         public static void LogMapperManagementClientException(this ILogger logger, Exception exception = null)
            => MapperManagementClientException(logger, exception);
                
         public static void LogAgencyAccountAddedNotificationFailure(this ILogger logger, int AgencyId, string Error, Exception exception = null)
            => AgencyAccountAddedNotificationFailure(logger, AgencyId, Error, exception);
                
         public static void LogAgentRegistrationNotificationFailure(this ILogger logger, string Error, Exception exception = null)
            => AgentRegistrationNotificationFailure(logger, Error, exception);
                
         public static void LogChildAgencyRegistrationNotificationFailure(this ILogger logger, Exception exception = null)
            => ChildAgencyRegistrationNotificationFailure(logger, exception);
                
         public static void LogAgencyAccountSubtractedNotificationFailure(this ILogger logger, int AgencyId, string Error, Exception exception = null)
            => AgencyAccountSubtractedNotificationFailure(logger, AgencyId, Error, exception);
                
         public static void LogAgencyAccountIncreasedManuallyNotificationFailure(this ILogger logger, int AgencyId, string Error, Exception exception = null)
            => AgencyAccountIncreasedManuallyNotificationFailure(logger, AgencyId, Error, exception);
                
         public static void LogAgencyAccountDecreasedManuallyNotificationFailure(this ILogger logger, int AgencyId, string Error, Exception exception = null)
            => AgencyAccountDecreasedManuallyNotificationFailure(logger, AgencyId, Error, exception);
                
         public static void LogExternalPaymentLinkGenerationSuccess(this ILogger logger, string Email, Exception exception = null)
            => ExternalPaymentLinkGenerationSuccess(logger, Email, exception);
                
         public static void LogExternalPaymentLinkGenerationFailed(this ILogger logger, string Email, string Error, Exception exception = null)
            => ExternalPaymentLinkGenerationFailed(logger, Email, Error, exception);
                
         public static void LogGetAccommodationByHtIdFailed(this ILogger logger, string HtId, string Error, Exception exception = null)
            => GetAccommodationByHtIdFailed(logger, HtId, Error, exception);
                
         public static void LogSendConfirmationEmailFailure(this ILogger logger, string ReferenceCode, Exception exception = null)
            => SendConfirmationEmailFailure(logger, ReferenceCode, exception);
                
         public static void LogConnectorClientUnexpectedResponse(this ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response, Exception exception = null)
            => ConnectorClientUnexpectedResponse(logger, StatusCode, Uri, Response, exception);
                
         public static void LogMapperClientUnexpectedResponse(this ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response, Exception exception = null)
            => MapperClientUnexpectedResponse(logger, StatusCode, Uri, Response, exception);
                
         public static void LogMapperClientRequestTimeout(this ILogger logger, Exception exception = null)
            => MapperClientRequestTimeout(logger, exception);
                
         public static void LogMapperManagementClientUnexpectedResponse(this ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response, Exception exception = null)
            => MapperManagementClientUnexpectedResponse(logger, StatusCode, Uri, Response, exception);
                
         public static void LogMapperManagementClientRequestTimeout(this ILogger logger, Exception exception = null)
            => MapperManagementClientRequestTimeout(logger, exception);
                
         public static void LogMarkupPolicyStorageRefreshed(this ILogger logger, int Count, Exception exception = null)
            => MarkupPolicyStorageRefreshed(logger, Count, exception);
                
         public static void LogMarkupPolicyStorageUpdateCompleted(this ILogger logger, Exception exception = null)
            => MarkupPolicyStorageUpdateCompleted(logger, exception);
                
         public static void LogMarkupPolicyStorageUpdateFailed(this ILogger logger, Exception exception = null)
            => MarkupPolicyStorageUpdateFailed(logger, exception);
                
         public static void LogCurrencyConversionFailed(this ILogger logger, HappyTravel.Money.Enums.Currencies Source, HappyTravel.Money.Enums.Currencies Target, string Error, Exception exception = null)
            => CurrencyConversionFailed(logger, Source, Target, Error, exception);
                
         public static void LogNGeniusWebhookProcessingStarted(this ILogger logger, Exception exception = null)
            => NGeniusWebhookProcessingStarted(logger, exception);
                
         public static void LogNGeniusWebhookPaymentUpdate(this ILogger logger, Exception exception = null)
            => NGeniusWebhookPaymentUpdate(logger, exception);
                
         public static void LogNGeniusWebhookPaymentLinkUpdate(this ILogger logger, Exception exception = null)
            => NGeniusWebhookPaymentLinkUpdate(logger, exception);
                
         public static void LogBookingExceededTimeLimit(this ILogger logger, string ReferenceCode, Exception exception = null)
            => BookingExceededTimeLimit(logger, ReferenceCode, exception);
                
         public static void LogInvoiceGenerated(this ILogger logger, string InvoiceNumber, string ReferenceCode, Exception exception = null)
            => InvoiceGenerated(logger, InvoiceNumber, ReferenceCode, exception);
                
         public static void LogCreditCardBookingFlowStarted(this ILogger logger, string HtId, Exception exception = null)
            => CreditCardBookingFlowStarted(logger, HtId, exception);
                
         public static void LogVccIssueStarted(this ILogger logger, string ReferenceCode, Exception exception = null)
            => VccIssueStarted(logger, ReferenceCode, exception);
                
         public static void LogCreditCardAuthorizationStarted(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardAuthorizationStarted(logger, ReferenceCode, exception);
                
         public static void LogCreditCardAuthorizationSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardAuthorizationSuccess(logger, ReferenceCode, exception);
                
         public static void LogCreditCardAuthorizationFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => CreditCardAuthorizationFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogCreditCardCapturingStarted(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardCapturingStarted(logger, ReferenceCode, exception);
                
         public static void LogCreditCardCapturingSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardCapturingSuccess(logger, ReferenceCode, exception);
                
         public static void LogCreditCardCapturingFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => CreditCardCapturingFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogCreditCardVoidingStarted(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardVoidingStarted(logger, ReferenceCode, exception);
                
         public static void LogCreditCardVoidingSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardVoidingSuccess(logger, ReferenceCode, exception);
                
         public static void LogCreditCardVoidingFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => CreditCardVoidingFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogCreditCardRefundingStarted(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardRefundingStarted(logger, ReferenceCode, exception);
                
         public static void LogCreditCardRefundingSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardRefundingSuccess(logger, ReferenceCode, exception);
                
         public static void LogCreditCardRefundingFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => CreditCardRefundingFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogCreditCardProcessingPaymentStarted(this ILogger logger, Exception exception = null)
            => CreditCardProcessingPaymentStarted(logger, exception);
                
         public static void LogCreditCardProcessingPaymentSuccess(this ILogger logger, Exception exception = null)
            => CreditCardProcessingPaymentSuccess(logger, exception);
                
         public static void LogCreditCardProcessingPaymentFailure(this ILogger logger, string Error, Exception exception = null)
            => CreditCardProcessingPaymentFailure(logger, Error, exception);
                
         public static void LogDiscountStorageRefreshed(this ILogger logger, int Count, Exception exception = null)
            => DiscountStorageRefreshed(logger, Count, exception);
                
         public static void LogDiscountStorageUpdateCompleted(this ILogger logger, Exception exception = null)
            => DiscountStorageUpdateCompleted(logger, exception);
                
         public static void LogDiscountStorageUpdateFailed(this ILogger logger, Exception exception = null)
            => DiscountStorageUpdateFailed(logger, exception);
                
         public static void LogMarkupPoliciesSumLessThanZero(this ILogger logger, int AgentId, decimal TotalPercentage, string Policies, Exception exception = null)
            => MarkupPoliciesSumLessThanZero(logger, AgentId, TotalPercentage, Policies, exception);
                
         public static void LogTotalDeadlineShiftIsPositive(this ILogger logger, int AgentId, int AgencyId, int RootShift, int AgencyShift, int AgentShift, Exception exception = null)
            => TotalDeadlineShiftIsPositive(logger, AgentId, AgencyId, RootShift, AgencyShift, AgentShift, exception);
    
    
        
        private static readonly Action<ILogger, Exception> GeoCoderException;
        
        private static readonly Action<ILogger, HappyTravel.Edo.Common.Enums.UserInvitationTypes, string, Exception> InvitationCreated;
        
        private static readonly Action<ILogger, string, Exception> AgentRegistrationFailed;
        
        private static readonly Action<ILogger, string, Exception> AgentRegistrationSuccess;
        
        private static readonly Action<ILogger, Exception> PayfortClientException;
        
        private static readonly Action<ILogger, int, int, Exception> AgencyAccountCreationSuccess;
        
        private static readonly Action<ILogger, int, string, Exception> AgencyAccountCreationFailed;
        
        private static readonly Action<ILogger, string, string, Exception> EntityLockFailed;
        
        private static readonly Action<ILogger, string, Exception> PayfortError;
        
        private static readonly Action<ILogger, string, Exception> ExternalPaymentLinkSendSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> ExternalPaymentLinkSendFailed;
        
        private static readonly Action<ILogger, string, Exception> UnableGetBookingDetailsFromNetstormingXml;
        
        private static readonly Action<ILogger, Exception> UnableToAcceptNetstormingRequest;
        
        private static readonly Action<ILogger, string, string, Exception> BookingFinalizationFailure;
        
        private static readonly Action<ILogger, string, Exception> BookingFinalizationPaymentFailure;
        
        private static readonly Action<ILogger, string, Exception> BookingFinalizationSuccess;
        
        private static readonly Action<ILogger, Exception> BookingFinalizationException;
        
        private static readonly Action<ILogger, string, Exception> BookingResponseProcessFailure;
        
        private static readonly Action<ILogger, string, string, Exception> BookingResponseProcessSuccess;
        
        private static readonly Action<ILogger, string, HappyTravel.Edo.Common.Enums.BookingStatuses, Exception> BookingResponseProcessStarted;
        
        private static readonly Action<ILogger, string, string, Exception> BookingCancelFailure;
        
        private static readonly Action<ILogger, string, Exception> BookingCancelSuccess;
        
        private static readonly Action<ILogger, string, Exception> BookingAlreadyCancelled;
        
        private static readonly Action<ILogger, string, Exception> BookingRegistrationSuccess;
        
        private static readonly Action<ILogger, string, string, string, string, Exception> BookingRegistrationFailure;
        
        private static readonly Action<ILogger, string, Exception> BookingByAccountSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> BookingByAccountFailure;
        
        private static readonly Action<ILogger, string, Exception> BookingByAccountStarted;
        
        private static readonly Action<ILogger, string, Exception> BookingByOfflinePaymentSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> BookingByOfflinePaymentFailure;
        
        private static readonly Action<ILogger, string, Exception> BookingByOfflinePaymentStarted;
        
        private static readonly Action<ILogger, string, HappyTravel.Edo.Common.Enums.BookingStatuses, HappyTravel.EdoContracts.Accommodations.Enums.BookingStatusCodes, Exception> BookingRefreshStatusSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> BookingRefreshStatusFailure;
        
        private static readonly Action<ILogger, string, string, Exception> BookingConfirmationFailure;
        
        private static readonly Action<ILogger, System.Nullable<int>, string, Exception> BookingEvaluationFailure;
        
        private static readonly Action<ILogger, Exception> BookingEvaluationCancellationPoliciesFailure;
        
        private static readonly Action<ILogger, Exception> ExternalAdministratorAuthorizationSuccess;
        
        private static readonly Action<ILogger, Exception> AdministratorAuthorizationFailure;
        
        private static readonly Action<ILogger, string, Exception> InternalAdministratorAuthorizationSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> AgentAuthorizationSuccess;
        
        private static readonly Action<ILogger, string, Exception> AgentAuthorizationFailure;
        
        private static readonly Action<ILogger, string, Exception> ServiceAccountAuthorizationSuccess;
        
        private static readonly Action<ILogger, string, Exception> ServiceAccountAuthorizationFailure;
        
        private static readonly Action<ILogger, Exception> LocationNormalized;
        
        private static readonly Action<ILogger, string, string, string[], string, int, Exception> MultiSupplierAvailabilitySearchStarted;
        
        private static readonly Action<ILogger, System.Guid, string, Exception> SupplierAvailabilitySearchStarted;
        
        private static readonly Action<ILogger, System.Guid, string, int, Exception> SupplierAvailabilitySearchSuccess;
        
        private static readonly Action<ILogger, System.Guid, string, HappyTravel.Edo.Api.Models.Availabilities.AvailabilitySearchTaskState, string, Exception> SupplierAvailabilitySearchFailure;
        
        private static readonly Action<ILogger, string, Exception> SupplierAvailabilitySearchException;
        
        private static readonly Action<ILogger, string, Exception> AgencyVerificationStateAuthorizationSuccess;
        
        private static readonly Action<ILogger, string, HappyTravel.Edo.Common.Enums.AgencyVerificationStates, Exception> AgencyVerificationStateAuthorizationFailure;
        
        private static readonly Action<ILogger, Exception> DefaultLanguageKeyIsMissingInFieldOfLocationsTable;
        
        private static readonly Action<ILogger, string, string, Exception> ConnectorClientException;
        
        private static readonly Action<ILogger, string, string, string, System.Nullable<int>, Exception> SupplierConnectorRequestError;
        
        private static readonly Action<ILogger, string, string, Exception> SupplierConnectorRequestSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> SupplierConnectorRequestStarted;
        
        private static readonly Action<ILogger, string, string, System.DateTime, Exception> GetTokenForConnectorError;
        
        private static readonly Action<ILogger, string, Exception> UnauthorizedConnectorResponse;
        
        private static readonly Action<ILogger, string, Exception> CaptureMoneyForBookingSuccess;
        
        private static readonly Action<ILogger, string, HappyTravel.Edo.Common.Enums.PaymentTypes, Exception> CaptureMoneyForBookingFailure;
        
        private static readonly Action<ILogger, string, Exception> ChargeMoneyForBookingSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> ChargeMoneyForBookingFailure;
        
        private static readonly Action<ILogger, HappyTravel.Edo.Common.Enums.BookingPaymentStatuses, HappyTravel.Edo.Common.Enums.PaymentStatuses, string, string, Exception> ProcessPaymentChangesForBookingSuccess;
        
        private static readonly Action<ILogger, HappyTravel.Edo.Common.Enums.PaymentStatuses, string, string, Exception> ProcessPaymentChangesForBookingSkip;
        
        private static readonly Action<ILogger, HappyTravel.Edo.Common.Enums.PaymentStatuses, string, Exception> ProcessPaymentChangesForBookingFailure;
        
        private static readonly Action<ILogger, Exception> ElasticAnalyticsEventSendError;
        
        private static readonly Action<ILogger, Exception> MapperClientException;
        
        private static readonly Action<ILogger, string, int, string[], Exception> MapperClientErrorResponse;
        
        private static readonly Action<ILogger, Exception> MapperManagementClientException;
        
        private static readonly Action<ILogger, int, string, Exception> AgencyAccountAddedNotificationFailure;
        
        private static readonly Action<ILogger, string, Exception> AgentRegistrationNotificationFailure;
        
        private static readonly Action<ILogger, Exception> ChildAgencyRegistrationNotificationFailure;
        
        private static readonly Action<ILogger, int, string, Exception> AgencyAccountSubtractedNotificationFailure;
        
        private static readonly Action<ILogger, int, string, Exception> AgencyAccountIncreasedManuallyNotificationFailure;
        
        private static readonly Action<ILogger, int, string, Exception> AgencyAccountDecreasedManuallyNotificationFailure;
        
        private static readonly Action<ILogger, string, Exception> ExternalPaymentLinkGenerationSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> ExternalPaymentLinkGenerationFailed;
        
        private static readonly Action<ILogger, string, string, Exception> GetAccommodationByHtIdFailed;
        
        private static readonly Action<ILogger, string, Exception> SendConfirmationEmailFailure;
        
        private static readonly Action<ILogger, System.Net.HttpStatusCode, System.Uri, string, Exception> ConnectorClientUnexpectedResponse;
        
        private static readonly Action<ILogger, System.Net.HttpStatusCode, System.Uri, string, Exception> MapperClientUnexpectedResponse;
        
        private static readonly Action<ILogger, Exception> MapperClientRequestTimeout;
        
        private static readonly Action<ILogger, System.Net.HttpStatusCode, System.Uri, string, Exception> MapperManagementClientUnexpectedResponse;
        
        private static readonly Action<ILogger, Exception> MapperManagementClientRequestTimeout;
        
        private static readonly Action<ILogger, int, Exception> MarkupPolicyStorageRefreshed;
        
        private static readonly Action<ILogger, Exception> MarkupPolicyStorageUpdateCompleted;
        
        private static readonly Action<ILogger, Exception> MarkupPolicyStorageUpdateFailed;
        
        private static readonly Action<ILogger, HappyTravel.Money.Enums.Currencies, HappyTravel.Money.Enums.Currencies, string, Exception> CurrencyConversionFailed;
        
        private static readonly Action<ILogger, Exception> NGeniusWebhookProcessingStarted;
        
        private static readonly Action<ILogger, Exception> NGeniusWebhookPaymentUpdate;
        
        private static readonly Action<ILogger, Exception> NGeniusWebhookPaymentLinkUpdate;
        
        private static readonly Action<ILogger, string, Exception> BookingExceededTimeLimit;
        
        private static readonly Action<ILogger, string, string, Exception> InvoiceGenerated;
        
        private static readonly Action<ILogger, string, Exception> CreditCardBookingFlowStarted;
        
        private static readonly Action<ILogger, string, Exception> VccIssueStarted;
        
        private static readonly Action<ILogger, string, Exception> CreditCardAuthorizationStarted;
        
        private static readonly Action<ILogger, string, Exception> CreditCardAuthorizationSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> CreditCardAuthorizationFailure;
        
        private static readonly Action<ILogger, string, Exception> CreditCardCapturingStarted;
        
        private static readonly Action<ILogger, string, Exception> CreditCardCapturingSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> CreditCardCapturingFailure;
        
        private static readonly Action<ILogger, string, Exception> CreditCardVoidingStarted;
        
        private static readonly Action<ILogger, string, Exception> CreditCardVoidingSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> CreditCardVoidingFailure;
        
        private static readonly Action<ILogger, string, Exception> CreditCardRefundingStarted;
        
        private static readonly Action<ILogger, string, Exception> CreditCardRefundingSuccess;
        
        private static readonly Action<ILogger, string, string, Exception> CreditCardRefundingFailure;
        
        private static readonly Action<ILogger, Exception> CreditCardProcessingPaymentStarted;
        
        private static readonly Action<ILogger, Exception> CreditCardProcessingPaymentSuccess;
        
        private static readonly Action<ILogger, string, Exception> CreditCardProcessingPaymentFailure;
        
        private static readonly Action<ILogger, int, Exception> DiscountStorageRefreshed;
        
        private static readonly Action<ILogger, Exception> DiscountStorageUpdateCompleted;
        
        private static readonly Action<ILogger, Exception> DiscountStorageUpdateFailed;
        
        private static readonly Action<ILogger, int, decimal, string, Exception> MarkupPoliciesSumLessThanZero;
        
        private static readonly Action<ILogger, int, int, int, int, int, Exception> TotalDeadlineShiftIsPositive;
    }
}