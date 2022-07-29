using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(1001, LogLevel.Error, "Getting google response exception")]
    static partial void GeoCoderException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1006, LogLevel.Information, "The invitation with type {InvitationType} created for the user '{Email}'")]
    static partial void InvitationCreated(ILogger logger, HappyTravel.Edo.Common.Enums.UserInvitationTypes InvitationType, string Email);
    
    [LoggerMessage(1007, LogLevel.Warning, "Agent registration failed with error `{Error}`")]
    static partial void AgentRegistrationFailed(ILogger logger, string Error);
    
    [LoggerMessage(1008, LogLevel.Information, "Agent {Email} successfully registered")]
    static partial void AgentRegistrationSuccess(ILogger logger, string Email);
    
    [LoggerMessage(1009, LogLevel.Critical, "Payfort client exception")]
    static partial void PayfortClientException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1010, LogLevel.Information, "Successfully created account for agency: '{AgencyId}', account id: {AccountId}")]
    static partial void AgencyAccountCreationSuccess(ILogger logger, int AgencyId, int AccountId);
    
    [LoggerMessage(1011, LogLevel.Error, "Failed to create account for agency {AgencyId}, error {Error}")]
    static partial void AgencyAccountCreationFailed(ILogger logger, int AgencyId, string Error);
    
    [LoggerMessage(1012, LogLevel.Critical, "Failed to lock entity {EntityType} with id: {EntityId}")]
    static partial void EntityLockFailed(ILogger logger, string EntityType, string EntityId);
    
    [LoggerMessage(1013, LogLevel.Error, "Error deserializing payfort response: '{Content}'")]
    static partial void PayfortError(ILogger logger, string Content);
    
    [LoggerMessage(1014, LogLevel.Information, "Successfully sent e-mail to {Email}")]
    static partial void ExternalPaymentLinkSendSuccess(ILogger logger, string Email);
    
    [LoggerMessage(1015, LogLevel.Error, "Error sending email to {Email}: {Error}")]
    static partial void ExternalPaymentLinkSendFailed(ILogger logger, string Email, string Error);
    
    [LoggerMessage(1017, LogLevel.Warning, "Failed to get booking details from the Netstorming xml: {Xml}")]
    static partial void UnableGetBookingDetailsFromNetstormingXml(ILogger logger, string Xml);
    
    [LoggerMessage(1018, LogLevel.Warning, "Unable to accept netstorming request")]
    static partial void UnableToAcceptNetstormingRequest(ILogger logger);
    
    [LoggerMessage(1020, LogLevel.Error, "The booking finalization with the reference code: '{ReferenceCode}' has been failed with a message: {Message}")]
    static partial void BookingFinalizationFailure(ILogger logger, string ReferenceCode, string Message);
    
    [LoggerMessage(1021, LogLevel.Warning, "The booking with reference code: '{ReferenceCode}' hasn't been paid")]
    static partial void BookingFinalizationPaymentFailure(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1022, LogLevel.Information, "The booking finalization successfully completed. Reference code: '{ReferenceCode}'")]
    static partial void BookingFinalizationSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1023, LogLevel.Critical, "Booking finalization exception")]
    static partial void BookingFinalizationException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1030, LogLevel.Error, "Booking response process failure. Error: {Error}")]
    static partial void BookingResponseProcessFailure(ILogger logger, string Error);
    
    [LoggerMessage(1031, LogLevel.Information, "The booking response with the reference code '{ReferenceCode} has been processed with message {Message}")]
    static partial void BookingResponseProcessSuccess(ILogger logger, string ReferenceCode, string Message);
    
    [LoggerMessage(1032, LogLevel.Information, "Start the booking response processing with the reference code '{ReferenceCode}'. Old status: {Status}")]
    static partial void BookingResponseProcessStarted(ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.BookingStatuses Status);
    
    [LoggerMessage(1040, LogLevel.Critical, "Failed to cancel a booking with reference code: '{ReferenceCode}'. Error: {Error}")]
    static partial void BookingCancelFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1041, LogLevel.Information, "Successfully cancelled a booking with reference code: '{ReferenceCode}'")]
    static partial void BookingCancelSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1042, LogLevel.Information, "Skipping cancellation for a booking with reference code: '{ReferenceCode}'. Already cancelled.")]
    static partial void BookingAlreadyCancelled(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1050, LogLevel.Information, "Successfully registered a booking with reference code: '{ReferenceCode}")]
    static partial void BookingRegistrationSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1051, LogLevel.Error, "Failed to register a booking. HtId: {HtId}, Itinerary number: {ItineraryNumber}. Passenger name: {MainPassengerName}. Error: {Error}")]
    static partial void BookingRegistrationFailure(ILogger logger, string HtId, string ItineraryNumber, string MainPassengerName, string Error);
    
    [LoggerMessage(1060, LogLevel.Information, "Successfully booked using account. Reference code: '{ReferenceCode}'")]
    static partial void BookingByAccountSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1061, LogLevel.Error, "Failed to book using account. HtId: '{HtId}'. Error: {Error}")]
    static partial void BookingByAccountFailure(ILogger logger, string HtId, string Error);
    
    [LoggerMessage(1062, LogLevel.Information, "Book using account started. HtId: '{HtId}'")]
    static partial void BookingByAccountStarted(ILogger logger, string HtId);
    
    [LoggerMessage(1063, LogLevel.Information, "Successfully booked using offline payment. Reference code: '{ReferenceCode}'}")]
    static partial void BookingByOfflinePaymentSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1064, LogLevel.Error, "Failed to book using offline payment. HtId: '{HtId}'. Error: {Error}")]
    static partial void BookingByOfflinePaymentFailure(ILogger logger, string HtId, string Error);
    
    [LoggerMessage(1065, LogLevel.Information, "Book using offline payment started. HtId: '{HtId}'")]
    static partial void BookingByOfflinePaymentStarted(ILogger logger, string HtId);
    
    [LoggerMessage(1070, LogLevel.Information, "Successfully refreshed status for a booking with reference code: '{ReferenceCode}'. Old status: {OldStatus}. New status: {Status}")]
    static partial void BookingRefreshStatusSuccess(ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.BookingStatuses OldStatus, HappyTravel.EdoContracts.Accommodations.Enums.BookingStatusCodes Status);
    
    [LoggerMessage(1071, LogLevel.Error, "Failed to refresh status for a booking with reference code: '{ReferenceCode}' while getting info from a supplier. Error: {Error}")]
    static partial void BookingRefreshStatusFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1072, LogLevel.Critical, "Booking '{ReferenceCode} confirmation failed: '{Error}")]
    static partial void BookingConfirmationFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1073, LogLevel.Warning, "EvaluateOnConnector returned status code: {Status}, error: {Error}")]
    static partial void BookingEvaluationFailure(ILogger logger, System.Nullable<int> Status, string Error);
    
    [LoggerMessage(1074, LogLevel.Error, "EvaluateOnConnector returned cancellation policies with 0 penalty")]
    static partial void BookingEvaluationCancellationPoliciesFailure(ILogger logger);
    
    [LoggerMessage(1100, LogLevel.Debug, "Successfully authorized external administrator")]
    static partial void ExternalAdministratorAuthorizationSuccess(ILogger logger);
    
    [LoggerMessage(1101, LogLevel.Warning, "Administrator authorization failed")]
    static partial void AdministratorAuthorizationFailure(ILogger logger);
    
    [LoggerMessage(1103, LogLevel.Debug, "Successfully authorized administrator '{Email}'")]
    static partial void InternalAdministratorAuthorizationSuccess(ILogger logger, string Email);
    
    [LoggerMessage(1110, LogLevel.Debug, "Successfully authorized agent '{Email}' for '{Permissions}'")]
    static partial void AgentAuthorizationSuccess(ILogger logger, string Email, string Permissions);
    
    [LoggerMessage(1111, LogLevel.Warning, "Agent authorization failure: '{Error}'")]
    static partial void AgentAuthorizationFailure(ILogger logger, string Error);
    
    [LoggerMessage(1125, LogLevel.Debug, "Service account '{ClientId}' authorized successfully")]
    static partial void ServiceAccountAuthorizationSuccess(ILogger logger, string ClientId);
    
    [LoggerMessage(1126, LogLevel.Warning, "Service account authorization failed. Error: {Error}")]
    static partial void ServiceAccountAuthorizationFailure(ILogger logger, string Error);
    
    [LoggerMessage(1130, LogLevel.Information, "Location normalized")]
    static partial void LocationNormalized(ILogger logger);
    
    [LoggerMessage(1140, LogLevel.Information, "Starting availability search for {CheckInDate} - {CheckOutDate}. Locations: '{LocationHtIds}', nationality: '{Nationality}', rooms: {RoomCount}")]
    static partial void MultiSupplierAvailabilitySearchStarted(ILogger logger, string CheckInDate, string CheckOutDate, string[] LocationHtIds, string Nationality, int RoomCount);
    
    [LoggerMessage(1141, LogLevel.Information, "Availability search with id '{SearchId}' on supplier '{Supplier}' started")]
    static partial void SupplierAvailabilitySearchStarted(ILogger logger, System.Guid SearchId, string Supplier);
    
    [LoggerMessage(1142, LogLevel.Information, "Availability search with id '{SearchId}' on supplier '{Supplier}' finished successfully with '{ResultCount}' results")]
    static partial void SupplierAvailabilitySearchSuccess(ILogger logger, System.Guid SearchId, string Supplier, int ResultCount);
    
    [LoggerMessage(1143, LogLevel.Warning, "Availability search with id '{SearchId}' on supplier '{Supplier}' finished with state '{TaskState}', error '{Error}'")]
    static partial void SupplierAvailabilitySearchFailure(ILogger logger, System.Guid SearchId, string Supplier, HappyTravel.Edo.Api.Models.Availabilities.AvailabilitySearchTaskState TaskState, string Error);
    
    [LoggerMessage(1145, LogLevel.Error, "Supplier availability search exception on supplier '{Supplier}'")]
    static partial void SupplierAvailabilitySearchException(ILogger logger, System.Exception exception, string Supplier);
    
    [LoggerMessage(1146, LogLevel.Information, "Found cached results for supplier '{Supplier}' and searchId '{SearchId}'")]
    static partial void FoundCachedResults(ILogger logger, string Supplier, System.Guid SearchId);
    
    [LoggerMessage(1150, LogLevel.Debug, "Successfully checked agency verification state for agent {Email}")]
    static partial void AgencyVerificationStateAuthorizationSuccess(ILogger logger, string Email);
    
    [LoggerMessage(1151, LogLevel.Warning, "Agency of agent '{Email}' has wrong verification state {State}")]
    static partial void AgencyVerificationStateAuthorizationFailure(ILogger logger, string Email, HappyTravel.Edo.Common.Enums.AgencyVerificationStates State);
    
    [LoggerMessage(1200, LogLevel.Warning, "Default language key is missing in field of locations table")]
    static partial void DefaultLanguageKeyIsMissingInFieldOfLocationsTable(ILogger logger);
    
    [LoggerMessage(1300, LogLevel.Error, "Connector client exception, url {RequestUrl}, response: {Response}")]
    static partial void ConnectorClientException(ILogger logger, string RequestUrl, string Response);
    
    [LoggerMessage(1301, LogLevel.Error, "Error executing connector request to {Url}: '{Error}', operation: {OperationName}, status code: '{Status}'")]
    static partial void SupplierConnectorRequestError(ILogger logger, string Url, string Error, string OperationName, System.Nullable<int> Status);
    
    [LoggerMessage(1302, LogLevel.Information, "completed executing connector request to {Url}, operation {OperationName}")]
    static partial void SupplierConnectorRequestSuccess(ILogger logger, string Url, string OperationName);
    
    [LoggerMessage(1303, LogLevel.Information, "Started executing connector request to {Url}, operation {OperationName}")]
    static partial void SupplierConnectorRequestStarted(ILogger logger, string Url, string OperationName);
    
    [LoggerMessage(1310, LogLevel.Error, "Something went wrong while requesting the access token. Error: {Error}. Using existing token: '{Token}' with expiry date '{ExpiryDate}'")]
    static partial void GetTokenForConnectorError(ILogger logger, string Error, string Token, System.DateTime ExpiryDate);
    
    [LoggerMessage(1311, LogLevel.Debug, "Unauthorized response was returned from '{RequestUri}'. Refreshing token...")]
    static partial void UnauthorizedConnectorResponse(ILogger logger, string RequestUri);
    
    [LoggerMessage(1400, LogLevel.Information, "Successfully captured money for a booking with reference code: '{ReferenceCode}'")]
    static partial void CaptureMoneyForBookingSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1401, LogLevel.Error, "Failed to capture money for a booking with reference code: '{ReferenceCode}'. Error: Invalid payment method: {PaymentType}")]
    static partial void CaptureMoneyForBookingFailure(ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.PaymentTypes PaymentType);
    
    [LoggerMessage(1402, LogLevel.Information, "Successfully charged money for a booking with reference code: '{ReferenceCode}'")]
    static partial void ChargeMoneyForBookingSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1403, LogLevel.Error, "Failed to charge money for a booking with reference code: '{ReferenceCode} with error {Error}")]
    static partial void ChargeMoneyForBookingFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1410, LogLevel.Information, "Successfully processes payment changes. Old payment status: {OldPaymentStatus}. New payment status: {PaymentStatus}. Payment: '{PaymentReferenceCode}'. Booking reference code: '{BookingReferenceCode}'")]
    static partial void ProcessPaymentChangesForBookingSuccess(ILogger logger, HappyTravel.Edo.Common.Enums.BookingPaymentStatuses OldPaymentStatus, HappyTravel.Edo.Common.Enums.PaymentStatuses PaymentStatus, string PaymentReferenceCode, string BookingReferenceCode);
    
    [LoggerMessage(1411, LogLevel.Warning, "Skipped booking status update while processing payment changes. Payment status: {PaymentStatus}. Payment: '{PaymentReferenceCode}'. Booking reference code: '{BookingReferenceCode}'")]
    static partial void ProcessPaymentChangesForBookingSkip(ILogger logger, HappyTravel.Edo.Common.Enums.PaymentStatuses PaymentStatus, string PaymentReferenceCode, string BookingReferenceCode);
    
    [LoggerMessage(1412, LogLevel.Error, "Failed to process payment changes, could not find the corresponding booking. Payment status: {Status}. Payment: '{ReferenceCode}'")]
    static partial void ProcessPaymentChangesForBookingFailure(ILogger logger, HappyTravel.Edo.Common.Enums.PaymentStatuses Status, string ReferenceCode);
    
    [LoggerMessage(1501, LogLevel.Error, "Sending event to elasticsearch failed")]
    static partial void ElasticAnalyticsEventSendError(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1601, LogLevel.Error, "Mapper client exception")]
    static partial void MapperClientException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1602, LogLevel.Error, "Request to mapper failed: {Message}:{StatusCode}. Requested HtIds {HtIds}")]
    static partial void MapperClientErrorResponse(ILogger logger, string Message, int StatusCode, string[] HtIds);
    
    [LoggerMessage(1603, LogLevel.Error, "Mapper management client exception")]
    static partial void MapperManagementClientException(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1701, LogLevel.Error, "Agency {AgencyId} account added notification failed with error {Error}")]
    static partial void AgencyAccountAddedNotificationFailure(ILogger logger, int AgencyId, string Error);
    
    [LoggerMessage(1702, LogLevel.Error, "Agent registration notification failure with error {Error}")]
    static partial void AgentRegistrationNotificationFailure(ILogger logger, string Error);
    
    [LoggerMessage(1703, LogLevel.Error, "Child agency registration notification failed")]
    static partial void ChildAgencyRegistrationNotificationFailure(ILogger logger);
    
    [LoggerMessage(1704, LogLevel.Error, "Agency {AgencyId} account subtracted notification failed with error {Error}")]
    static partial void AgencyAccountSubtractedNotificationFailure(ILogger logger, int AgencyId, string Error);
    
    [LoggerMessage(1705, LogLevel.Error, "Agency {AgencyId} account increasedManually notification failed with error {Error}")]
    static partial void AgencyAccountIncreasedManuallyNotificationFailure(ILogger logger, int AgencyId, string Error);
    
    [LoggerMessage(1706, LogLevel.Error, "Agency {AgencyId} account decreasedManually notification failed with error {Error}")]
    static partial void AgencyAccountDecreasedManuallyNotificationFailure(ILogger logger, int AgencyId, string Error);
    
    [LoggerMessage(1707, LogLevel.Information, "Successfully generated payment link for {Email}")]
    static partial void ExternalPaymentLinkGenerationSuccess(ILogger logger, string Email);
    
    [LoggerMessage(1708, LogLevel.Error, "Error generating payment link for {Email}: {Error}")]
    static partial void ExternalPaymentLinkGenerationFailed(ILogger logger, string Email, string Error);
    
    [LoggerMessage(1709, LogLevel.Error, "Error getting accommodation for HtId '{HtId}': error: {Error}")]
    static partial void GetAccommodationByHtIdFailed(ILogger logger, string HtId, string Error);
    
    [LoggerMessage(1800, LogLevel.Error, "Error sending booking confirmation email to property owner. Received empty list of email addresses from mapper. Reference code {ReferenceCode}")]
    static partial void SendConfirmationEmailFailure(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1801, LogLevel.Error, "Unexpected response received from connector. StatusCode: `{StatusCode}`, request uri: `{Uri}`, response: {Response}")]
    static partial void ConnectorClientUnexpectedResponse(ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response);
    
    [LoggerMessage(1802, LogLevel.Error, "Unexpected response received from mapper. StatusCode: `{StatusCode}`, request uri: `{Uri}`, response: {Response}")]
    static partial void MapperClientUnexpectedResponse(ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response);
    
    [LoggerMessage(1803, LogLevel.Warning, "Request to mapper failed with timeout")]
    static partial void MapperClientRequestTimeout(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1804, LogLevel.Error, "Unexpected response received from a mapper management endpoint. StatusCode: `{StatusCode}`, request uri: `{Uri}`, response: {Response}")]
    static partial void MapperManagementClientUnexpectedResponse(ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response);
    
    [LoggerMessage(1806, LogLevel.Warning, "Request to a mapper management endpoint failed with timeout")]
    static partial void MapperManagementClientRequestTimeout(ILogger logger, System.Exception exception);
    
    [LoggerMessage(1090, LogLevel.Debug, "MarkupPolicyStorage refreshed. Was set {Count} entities")]
    static partial void MarkupPolicyStorageRefreshed(ILogger logger, int Count);
    
    [LoggerMessage(1091, LogLevel.Debug, "Markup policy storage update completed")]
    static partial void MarkupPolicyStorageUpdateCompleted(ILogger logger);
    
    [LoggerMessage(1092, LogLevel.Error, "Markup policy storage update failed")]
    static partial void MarkupPolicyStorageUpdateFailed(ILogger logger);
    
    [LoggerMessage(1093, LogLevel.Error, "Currency conversion failed. Source currency: `{Source}`, target currency: `{Target}`. Error: `{Error}`")]
    static partial void CurrencyConversionFailed(ILogger logger, HappyTravel.Money.Enums.Currencies Source, HappyTravel.Money.Enums.Currencies Target, string Error);
    
    [LoggerMessage(1095, LogLevel.Information, "NGenius webhook processing started")]
    static partial void NGeniusWebhookProcessingStarted(ILogger logger);
    
    [LoggerMessage(1096, LogLevel.Information, "Started updating payment by NGenius webhook")]
    static partial void NGeniusWebhookPaymentUpdate(ILogger logger);
    
    [LoggerMessage(1097, LogLevel.Information, "Started updating payment link by NGenius webhook")]
    static partial void NGeniusWebhookPaymentLinkUpdate(ILogger logger);
    
    [LoggerMessage(1098, LogLevel.Error, "Booking {ReferenceCode} exceeded time limit")]
    static partial void BookingExceededTimeLimit(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1100, LogLevel.Information, "Generated invoice number {InvoiceNumber} for booking {ReferenceCode}")]
    static partial void InvoiceGenerated(ILogger logger, string InvoiceNumber, string ReferenceCode);
    
    [LoggerMessage(1103, LogLevel.Information, "Сredit card booking flow started for htId {HtId}")]
    static partial void CreditCardBookingFlowStarted(ILogger logger, string HtId);
    
    [LoggerMessage(1104, LogLevel.Information, "Vcc issue started for booking {ReferenceCode}")]
    static partial void VccIssueStarted(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1110, LogLevel.Information, "Credit card authorization started. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardAuthorizationStarted(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1111, LogLevel.Information, "Credit card authorization success. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardAuthorizationSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1112, LogLevel.Information, "Credit card authorization failed. ReferenceCode: {ReferenceCode}, Error: {Error}")]
    static partial void CreditCardAuthorizationFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1113, LogLevel.Information, "Credit card capturing started. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardCapturingStarted(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1114, LogLevel.Information, "Credit card capturing success. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardCapturingSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1115, LogLevel.Error, "Credit card capturing failed. ReferenceCode: {ReferenceCode}, Error: {Error}")]
    static partial void CreditCardCapturingFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1116, LogLevel.Information, "Credit card voiding started. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardVoidingStarted(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1117, LogLevel.Information, "Credit card voiding success. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardVoidingSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1118, LogLevel.Error, "Credit card voiding failed. ReferenceCode: {ReferenceCode}, Error: {Error}")]
    static partial void CreditCardVoidingFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1119, LogLevel.Information, "Credit card refunding started. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardRefundingStarted(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1120, LogLevel.Information, "Credit card refunding success. ReferenceCode: {ReferenceCode}")]
    static partial void CreditCardRefundingSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(1121, LogLevel.Error, "Credit card refunding failed. ReferenceCode: {ReferenceCode}, Error: {Error}")]
    static partial void CreditCardRefundingFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(1122, LogLevel.Information, "Credit card processing payment started")]
    static partial void CreditCardProcessingPaymentStarted(ILogger logger);
    
    [LoggerMessage(1123, LogLevel.Information, "Credit card processing payment success")]
    static partial void CreditCardProcessingPaymentSuccess(ILogger logger);
    
    [LoggerMessage(1124, LogLevel.Error, "Credit card processing payment failed. Error: {Error}")]
    static partial void CreditCardProcessingPaymentFailure(ILogger logger, string Error);
    
    [LoggerMessage(1130, LogLevel.Debug, "Discount storage refreshed. Was set {Count} entities")]
    static partial void DiscountStorageRefreshed(ILogger logger, int Count);
    
    [LoggerMessage(1131, LogLevel.Debug, "Discount storage update completed")]
    static partial void DiscountStorageUpdateCompleted(ILogger logger);
    
    [LoggerMessage(1132, LogLevel.Error, "Discount storage update failed")]
    static partial void DiscountStorageUpdateFailed(ILogger logger);
    
    [LoggerMessage(1133, LogLevel.Warning, "Applyed markup policies' sum less than zero. AgentId: {AgentId}; Total percentage: {TotalPercentage}; Markup policies: {Policies}")]
    static partial void MarkupPoliciesSumLessThanZero(ILogger logger, int AgentId, decimal TotalPercentage, string Policies);
    
    [LoggerMessage(1134, LogLevel.Warning, "Total deadline shift is positive. AgentId: {AgentId}; AgencyId: {AgencyId}; RootShift: {RootShift}; AgencyShift: {AgencyShift}; AgentShift: {AgentShift};")]
    static partial void TotalDeadlineShiftIsPositive(ILogger logger, int AgentId, int AgencyId, int RootShift, int AgencyShift, int AgentShift);
    
    [LoggerMessage(1140, LogLevel.Warning, "Delaying connector client for {Delay}ms: '{Message}', then making retry {Retry}")]
    static partial void ConnectorClientDelay(ILogger logger, double Delay, string Message, int Retry);
    
    [LoggerMessage(1141, LogLevel.Warning, "{Message}")]
    static partial void ConnectorRequestFailedOnSecondStep(ILogger logger, string Message);
    
    [LoggerMessage(1142, LogLevel.Information, "Results for connector '{Connector}' were successfully refreshed")]
    static partial void ConnectorResultsWereRefreshed(ILogger logger, string Connector);
    
    [LoggerMessage(1150, LogLevel.Information, "Supplier update message received")]
    static partial void SupplierUpdateMessageReceived(ILogger logger);
    
    [LoggerMessage(1151, LogLevel.Information, "Grpc client for supplier {SupplierCode} updated")]
    static partial void GrpcSupplierClientUpdated(ILogger logger, string SupplierCode);
    
    
    
    public static void LogGeoCoderException(this ILogger logger, System.Exception exception)
        => GeoCoderException(logger, exception);
    
    public static void LogInvitationCreated(this ILogger logger, HappyTravel.Edo.Common.Enums.UserInvitationTypes InvitationType, string Email)
        => InvitationCreated(logger, InvitationType, Email);
    
    public static void LogAgentRegistrationFailed(this ILogger logger, string Error)
        => AgentRegistrationFailed(logger, Error);
    
    public static void LogAgentRegistrationSuccess(this ILogger logger, string Email)
        => AgentRegistrationSuccess(logger, Email);
    
    public static void LogPayfortClientException(this ILogger logger, System.Exception exception)
        => PayfortClientException(logger, exception);
    
    public static void LogAgencyAccountCreationSuccess(this ILogger logger, int AgencyId, int AccountId)
        => AgencyAccountCreationSuccess(logger, AgencyId, AccountId);
    
    public static void LogAgencyAccountCreationFailed(this ILogger logger, int AgencyId, string Error)
        => AgencyAccountCreationFailed(logger, AgencyId, Error);
    
    public static void LogEntityLockFailed(this ILogger logger, string EntityType, string EntityId)
        => EntityLockFailed(logger, EntityType, EntityId);
    
    public static void LogPayfortError(this ILogger logger, string Content)
        => PayfortError(logger, Content);
    
    public static void LogExternalPaymentLinkSendSuccess(this ILogger logger, string Email)
        => ExternalPaymentLinkSendSuccess(logger, Email);
    
    public static void LogExternalPaymentLinkSendFailed(this ILogger logger, string Email, string Error)
        => ExternalPaymentLinkSendFailed(logger, Email, Error);
    
    public static void LogUnableGetBookingDetailsFromNetstormingXml(this ILogger logger, string Xml)
        => UnableGetBookingDetailsFromNetstormingXml(logger, Xml);
    
    public static void LogUnableToAcceptNetstormingRequest(this ILogger logger)
        => UnableToAcceptNetstormingRequest(logger);
    
    public static void LogBookingFinalizationFailure(this ILogger logger, string ReferenceCode, string Message)
        => BookingFinalizationFailure(logger, ReferenceCode, Message);
    
    public static void LogBookingFinalizationPaymentFailure(this ILogger logger, string ReferenceCode)
        => BookingFinalizationPaymentFailure(logger, ReferenceCode);
    
    public static void LogBookingFinalizationSuccess(this ILogger logger, string ReferenceCode)
        => BookingFinalizationSuccess(logger, ReferenceCode);
    
    public static void LogBookingFinalizationException(this ILogger logger, System.Exception exception)
        => BookingFinalizationException(logger, exception);
    
    public static void LogBookingResponseProcessFailure(this ILogger logger, string Error)
        => BookingResponseProcessFailure(logger, Error);
    
    public static void LogBookingResponseProcessSuccess(this ILogger logger, string ReferenceCode, string Message)
        => BookingResponseProcessSuccess(logger, ReferenceCode, Message);
    
    public static void LogBookingResponseProcessStarted(this ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.BookingStatuses Status)
        => BookingResponseProcessStarted(logger, ReferenceCode, Status);
    
    public static void LogBookingCancelFailure(this ILogger logger, string ReferenceCode, string Error)
        => BookingCancelFailure(logger, ReferenceCode, Error);
    
    public static void LogBookingCancelSuccess(this ILogger logger, string ReferenceCode)
        => BookingCancelSuccess(logger, ReferenceCode);
    
    public static void LogBookingAlreadyCancelled(this ILogger logger, string ReferenceCode)
        => BookingAlreadyCancelled(logger, ReferenceCode);
    
    public static void LogBookingRegistrationSuccess(this ILogger logger, string ReferenceCode)
        => BookingRegistrationSuccess(logger, ReferenceCode);
    
    public static void LogBookingRegistrationFailure(this ILogger logger, string HtId, string ItineraryNumber, string MainPassengerName, string Error)
        => BookingRegistrationFailure(logger, HtId, ItineraryNumber, MainPassengerName, Error);
    
    public static void LogBookingByAccountSuccess(this ILogger logger, string ReferenceCode)
        => BookingByAccountSuccess(logger, ReferenceCode);
    
    public static void LogBookingByAccountFailure(this ILogger logger, string HtId, string Error)
        => BookingByAccountFailure(logger, HtId, Error);
    
    public static void LogBookingByAccountStarted(this ILogger logger, string HtId)
        => BookingByAccountStarted(logger, HtId);
    
    public static void LogBookingByOfflinePaymentSuccess(this ILogger logger, string ReferenceCode)
        => BookingByOfflinePaymentSuccess(logger, ReferenceCode);
    
    public static void LogBookingByOfflinePaymentFailure(this ILogger logger, string HtId, string Error)
        => BookingByOfflinePaymentFailure(logger, HtId, Error);
    
    public static void LogBookingByOfflinePaymentStarted(this ILogger logger, string HtId)
        => BookingByOfflinePaymentStarted(logger, HtId);
    
    public static void LogBookingRefreshStatusSuccess(this ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.BookingStatuses OldStatus, HappyTravel.EdoContracts.Accommodations.Enums.BookingStatusCodes Status)
        => BookingRefreshStatusSuccess(logger, ReferenceCode, OldStatus, Status);
    
    public static void LogBookingRefreshStatusFailure(this ILogger logger, string ReferenceCode, string Error)
        => BookingRefreshStatusFailure(logger, ReferenceCode, Error);
    
    public static void LogBookingConfirmationFailure(this ILogger logger, string ReferenceCode, string Error)
        => BookingConfirmationFailure(logger, ReferenceCode, Error);
    
    public static void LogBookingEvaluationFailure(this ILogger logger, System.Nullable<int> Status, string Error)
        => BookingEvaluationFailure(logger, Status, Error);
    
    public static void LogBookingEvaluationCancellationPoliciesFailure(this ILogger logger)
        => BookingEvaluationCancellationPoliciesFailure(logger);
    
    public static void LogExternalAdministratorAuthorizationSuccess(this ILogger logger)
        => ExternalAdministratorAuthorizationSuccess(logger);
    
    public static void LogAdministratorAuthorizationFailure(this ILogger logger)
        => AdministratorAuthorizationFailure(logger);
    
    public static void LogInternalAdministratorAuthorizationSuccess(this ILogger logger, string Email)
        => InternalAdministratorAuthorizationSuccess(logger, Email);
    
    public static void LogAgentAuthorizationSuccess(this ILogger logger, string Email, string Permissions)
        => AgentAuthorizationSuccess(logger, Email, Permissions);
    
    public static void LogAgentAuthorizationFailure(this ILogger logger, string Error)
        => AgentAuthorizationFailure(logger, Error);
    
    public static void LogServiceAccountAuthorizationSuccess(this ILogger logger, string ClientId)
        => ServiceAccountAuthorizationSuccess(logger, ClientId);
    
    public static void LogServiceAccountAuthorizationFailure(this ILogger logger, string Error)
        => ServiceAccountAuthorizationFailure(logger, Error);
    
    public static void LogLocationNormalized(this ILogger logger)
        => LocationNormalized(logger);
    
    public static void LogMultiSupplierAvailabilitySearchStarted(this ILogger logger, string CheckInDate, string CheckOutDate, string[] LocationHtIds, string Nationality, int RoomCount)
        => MultiSupplierAvailabilitySearchStarted(logger, CheckInDate, CheckOutDate, LocationHtIds, Nationality, RoomCount);
    
    public static void LogSupplierAvailabilitySearchStarted(this ILogger logger, System.Guid SearchId, string Supplier)
        => SupplierAvailabilitySearchStarted(logger, SearchId, Supplier);
    
    public static void LogSupplierAvailabilitySearchSuccess(this ILogger logger, System.Guid SearchId, string Supplier, int ResultCount)
        => SupplierAvailabilitySearchSuccess(logger, SearchId, Supplier, ResultCount);
    
    public static void LogSupplierAvailabilitySearchFailure(this ILogger logger, System.Guid SearchId, string Supplier, HappyTravel.Edo.Api.Models.Availabilities.AvailabilitySearchTaskState TaskState, string Error)
        => SupplierAvailabilitySearchFailure(logger, SearchId, Supplier, TaskState, Error);
    
    public static void LogSupplierAvailabilitySearchException(this ILogger logger, System.Exception exception, string Supplier)
        => SupplierAvailabilitySearchException(logger, exception, Supplier);
    
    public static void LogFoundCachedResults(this ILogger logger, string Supplier, System.Guid SearchId)
        => FoundCachedResults(logger, Supplier, SearchId);
    
    public static void LogAgencyVerificationStateAuthorizationSuccess(this ILogger logger, string Email)
        => AgencyVerificationStateAuthorizationSuccess(logger, Email);
    
    public static void LogAgencyVerificationStateAuthorizationFailure(this ILogger logger, string Email, HappyTravel.Edo.Common.Enums.AgencyVerificationStates State)
        => AgencyVerificationStateAuthorizationFailure(logger, Email, State);
    
    public static void LogDefaultLanguageKeyIsMissingInFieldOfLocationsTable(this ILogger logger)
        => DefaultLanguageKeyIsMissingInFieldOfLocationsTable(logger);
    
    public static void LogConnectorClientException(this ILogger logger, string RequestUrl, string Response)
        => ConnectorClientException(logger, RequestUrl, Response);
    
    public static void LogSupplierConnectorRequestError(this ILogger logger, string Url, string Error, string OperationName, System.Nullable<int> Status)
        => SupplierConnectorRequestError(logger, Url, Error, OperationName, Status);
    
    public static void LogSupplierConnectorRequestSuccess(this ILogger logger, string Url, string OperationName)
        => SupplierConnectorRequestSuccess(logger, Url, OperationName);
    
    public static void LogSupplierConnectorRequestStarted(this ILogger logger, string Url, string OperationName)
        => SupplierConnectorRequestStarted(logger, Url, OperationName);
    
    public static void LogGetTokenForConnectorError(this ILogger logger, string Error, string Token, System.DateTime ExpiryDate)
        => GetTokenForConnectorError(logger, Error, Token, ExpiryDate);
    
    public static void LogUnauthorizedConnectorResponse(this ILogger logger, string RequestUri)
        => UnauthorizedConnectorResponse(logger, RequestUri);
    
    public static void LogCaptureMoneyForBookingSuccess(this ILogger logger, string ReferenceCode)
        => CaptureMoneyForBookingSuccess(logger, ReferenceCode);
    
    public static void LogCaptureMoneyForBookingFailure(this ILogger logger, string ReferenceCode, HappyTravel.Edo.Common.Enums.PaymentTypes PaymentType)
        => CaptureMoneyForBookingFailure(logger, ReferenceCode, PaymentType);
    
    public static void LogChargeMoneyForBookingSuccess(this ILogger logger, string ReferenceCode)
        => ChargeMoneyForBookingSuccess(logger, ReferenceCode);
    
    public static void LogChargeMoneyForBookingFailure(this ILogger logger, string ReferenceCode, string Error)
        => ChargeMoneyForBookingFailure(logger, ReferenceCode, Error);
    
    public static void LogProcessPaymentChangesForBookingSuccess(this ILogger logger, HappyTravel.Edo.Common.Enums.BookingPaymentStatuses OldPaymentStatus, HappyTravel.Edo.Common.Enums.PaymentStatuses PaymentStatus, string PaymentReferenceCode, string BookingReferenceCode)
        => ProcessPaymentChangesForBookingSuccess(logger, OldPaymentStatus, PaymentStatus, PaymentReferenceCode, BookingReferenceCode);
    
    public static void LogProcessPaymentChangesForBookingSkip(this ILogger logger, HappyTravel.Edo.Common.Enums.PaymentStatuses PaymentStatus, string PaymentReferenceCode, string BookingReferenceCode)
        => ProcessPaymentChangesForBookingSkip(logger, PaymentStatus, PaymentReferenceCode, BookingReferenceCode);
    
    public static void LogProcessPaymentChangesForBookingFailure(this ILogger logger, HappyTravel.Edo.Common.Enums.PaymentStatuses Status, string ReferenceCode)
        => ProcessPaymentChangesForBookingFailure(logger, Status, ReferenceCode);
    
    public static void LogElasticAnalyticsEventSendError(this ILogger logger, System.Exception exception)
        => ElasticAnalyticsEventSendError(logger, exception);
    
    public static void LogMapperClientException(this ILogger logger, System.Exception exception)
        => MapperClientException(logger, exception);
    
    public static void LogMapperClientErrorResponse(this ILogger logger, string Message, int StatusCode, string[] HtIds)
        => MapperClientErrorResponse(logger, Message, StatusCode, HtIds);
    
    public static void LogMapperManagementClientException(this ILogger logger, System.Exception exception)
        => MapperManagementClientException(logger, exception);
    
    public static void LogAgencyAccountAddedNotificationFailure(this ILogger logger, int AgencyId, string Error)
        => AgencyAccountAddedNotificationFailure(logger, AgencyId, Error);
    
    public static void LogAgentRegistrationNotificationFailure(this ILogger logger, string Error)
        => AgentRegistrationNotificationFailure(logger, Error);
    
    public static void LogChildAgencyRegistrationNotificationFailure(this ILogger logger)
        => ChildAgencyRegistrationNotificationFailure(logger);
    
    public static void LogAgencyAccountSubtractedNotificationFailure(this ILogger logger, int AgencyId, string Error)
        => AgencyAccountSubtractedNotificationFailure(logger, AgencyId, Error);
    
    public static void LogAgencyAccountIncreasedManuallyNotificationFailure(this ILogger logger, int AgencyId, string Error)
        => AgencyAccountIncreasedManuallyNotificationFailure(logger, AgencyId, Error);
    
    public static void LogAgencyAccountDecreasedManuallyNotificationFailure(this ILogger logger, int AgencyId, string Error)
        => AgencyAccountDecreasedManuallyNotificationFailure(logger, AgencyId, Error);
    
    public static void LogExternalPaymentLinkGenerationSuccess(this ILogger logger, string Email)
        => ExternalPaymentLinkGenerationSuccess(logger, Email);
    
    public static void LogExternalPaymentLinkGenerationFailed(this ILogger logger, string Email, string Error)
        => ExternalPaymentLinkGenerationFailed(logger, Email, Error);
    
    public static void LogGetAccommodationByHtIdFailed(this ILogger logger, string HtId, string Error)
        => GetAccommodationByHtIdFailed(logger, HtId, Error);
    
    public static void LogSendConfirmationEmailFailure(this ILogger logger, string ReferenceCode)
        => SendConfirmationEmailFailure(logger, ReferenceCode);
    
    public static void LogConnectorClientUnexpectedResponse(this ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response)
        => ConnectorClientUnexpectedResponse(logger, StatusCode, Uri, Response);
    
    public static void LogMapperClientUnexpectedResponse(this ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response)
        => MapperClientUnexpectedResponse(logger, StatusCode, Uri, Response);
    
    public static void LogMapperClientRequestTimeout(this ILogger logger, System.Exception exception)
        => MapperClientRequestTimeout(logger, exception);
    
    public static void LogMapperManagementClientUnexpectedResponse(this ILogger logger, System.Net.HttpStatusCode StatusCode, System.Uri Uri, string Response)
        => MapperManagementClientUnexpectedResponse(logger, StatusCode, Uri, Response);
    
    public static void LogMapperManagementClientRequestTimeout(this ILogger logger, System.Exception exception)
        => MapperManagementClientRequestTimeout(logger, exception);
    
    public static void LogMarkupPolicyStorageRefreshed(this ILogger logger, int Count)
        => MarkupPolicyStorageRefreshed(logger, Count);
    
    public static void LogMarkupPolicyStorageUpdateCompleted(this ILogger logger)
        => MarkupPolicyStorageUpdateCompleted(logger);
    
    public static void LogMarkupPolicyStorageUpdateFailed(this ILogger logger)
        => MarkupPolicyStorageUpdateFailed(logger);
    
    public static void LogCurrencyConversionFailed(this ILogger logger, HappyTravel.Money.Enums.Currencies Source, HappyTravel.Money.Enums.Currencies Target, string Error)
        => CurrencyConversionFailed(logger, Source, Target, Error);
    
    public static void LogNGeniusWebhookProcessingStarted(this ILogger logger)
        => NGeniusWebhookProcessingStarted(logger);
    
    public static void LogNGeniusWebhookPaymentUpdate(this ILogger logger)
        => NGeniusWebhookPaymentUpdate(logger);
    
    public static void LogNGeniusWebhookPaymentLinkUpdate(this ILogger logger)
        => NGeniusWebhookPaymentLinkUpdate(logger);
    
    public static void LogBookingExceededTimeLimit(this ILogger logger, string ReferenceCode)
        => BookingExceededTimeLimit(logger, ReferenceCode);
    
    public static void LogInvoiceGenerated(this ILogger logger, string InvoiceNumber, string ReferenceCode)
        => InvoiceGenerated(logger, InvoiceNumber, ReferenceCode);
    
    public static void LogCreditCardBookingFlowStarted(this ILogger logger, string HtId)
        => CreditCardBookingFlowStarted(logger, HtId);
    
    public static void LogVccIssueStarted(this ILogger logger, string ReferenceCode)
        => VccIssueStarted(logger, ReferenceCode);
    
    public static void LogCreditCardAuthorizationStarted(this ILogger logger, string ReferenceCode)
        => CreditCardAuthorizationStarted(logger, ReferenceCode);
    
    public static void LogCreditCardAuthorizationSuccess(this ILogger logger, string ReferenceCode)
        => CreditCardAuthorizationSuccess(logger, ReferenceCode);
    
    public static void LogCreditCardAuthorizationFailure(this ILogger logger, string ReferenceCode, string Error)
        => CreditCardAuthorizationFailure(logger, ReferenceCode, Error);
    
    public static void LogCreditCardCapturingStarted(this ILogger logger, string ReferenceCode)
        => CreditCardCapturingStarted(logger, ReferenceCode);
    
    public static void LogCreditCardCapturingSuccess(this ILogger logger, string ReferenceCode)
        => CreditCardCapturingSuccess(logger, ReferenceCode);
    
    public static void LogCreditCardCapturingFailure(this ILogger logger, string ReferenceCode, string Error)
        => CreditCardCapturingFailure(logger, ReferenceCode, Error);
    
    public static void LogCreditCardVoidingStarted(this ILogger logger, string ReferenceCode)
        => CreditCardVoidingStarted(logger, ReferenceCode);
    
    public static void LogCreditCardVoidingSuccess(this ILogger logger, string ReferenceCode)
        => CreditCardVoidingSuccess(logger, ReferenceCode);
    
    public static void LogCreditCardVoidingFailure(this ILogger logger, string ReferenceCode, string Error)
        => CreditCardVoidingFailure(logger, ReferenceCode, Error);
    
    public static void LogCreditCardRefundingStarted(this ILogger logger, string ReferenceCode)
        => CreditCardRefundingStarted(logger, ReferenceCode);
    
    public static void LogCreditCardRefundingSuccess(this ILogger logger, string ReferenceCode)
        => CreditCardRefundingSuccess(logger, ReferenceCode);
    
    public static void LogCreditCardRefundingFailure(this ILogger logger, string ReferenceCode, string Error)
        => CreditCardRefundingFailure(logger, ReferenceCode, Error);
    
    public static void LogCreditCardProcessingPaymentStarted(this ILogger logger)
        => CreditCardProcessingPaymentStarted(logger);
    
    public static void LogCreditCardProcessingPaymentSuccess(this ILogger logger)
        => CreditCardProcessingPaymentSuccess(logger);
    
    public static void LogCreditCardProcessingPaymentFailure(this ILogger logger, string Error)
        => CreditCardProcessingPaymentFailure(logger, Error);
    
    public static void LogDiscountStorageRefreshed(this ILogger logger, int Count)
        => DiscountStorageRefreshed(logger, Count);
    
    public static void LogDiscountStorageUpdateCompleted(this ILogger logger)
        => DiscountStorageUpdateCompleted(logger);
    
    public static void LogDiscountStorageUpdateFailed(this ILogger logger)
        => DiscountStorageUpdateFailed(logger);
    
    public static void LogMarkupPoliciesSumLessThanZero(this ILogger logger, int AgentId, decimal TotalPercentage, string Policies)
        => MarkupPoliciesSumLessThanZero(logger, AgentId, TotalPercentage, Policies);
    
    public static void LogTotalDeadlineShiftIsPositive(this ILogger logger, int AgentId, int AgencyId, int RootShift, int AgencyShift, int AgentShift)
        => TotalDeadlineShiftIsPositive(logger, AgentId, AgencyId, RootShift, AgencyShift, AgentShift);
    
    public static void LogConnectorClientDelay(this ILogger logger, double Delay, string Message, int Retry)
        => ConnectorClientDelay(logger, Delay, Message, Retry);
    
    public static void LogConnectorRequestFailedOnSecondStep(this ILogger logger, string Message)
        => ConnectorRequestFailedOnSecondStep(logger, Message);
    
    public static void LogConnectorResultsWereRefreshed(this ILogger logger, string Connector)
        => ConnectorResultsWereRefreshed(logger, Connector);
    
    public static void LogSupplierUpdateMessageReceived(this ILogger logger)
        => SupplierUpdateMessageReceived(logger);
    
    public static void LogGrpcSupplierClientUpdated(this ILogger logger, string SupplierCode)
        => GrpcSupplierClientUpdated(logger, SupplierCode);
}