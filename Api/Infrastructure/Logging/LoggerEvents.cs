namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    //DO NOT REMOVE NEVER USED CODES FOR COMPATIBILITY WITH LOGS
    public enum LoggerEvents
    {
        GeocoderException = 1001,
        AvailabilityCheckException = 1002,
        DataProviderClientException = 1003,
        DataProviderRequestError = 1004,
        InvitationCreatedInformation = 1006,
        AgentRegistrationFailed = 1007,
        AgentRegistrationSuccess = 1008,
        PayfortClientException = 1009,
        PaymentAccountCreationSuccess = 1010,
        PaymentAccountCreationFailed = 1011,
        EntityLockFailed = 1012,
        PayfortError = 1013,
        ExternalPaymentLinkSendSuccess = 1014,
        ExternalPaymentLinkSendFailed = 1015,
        UnableCaptureWholeAmountForBooking = 1016,
        UnableGetBookingDetailsFromNetstormingXml = 1017,
        UnableToAcceptNetstormingRequest = 1018,
        BookingFinalizationFailure = 1020,
        BookingFinalizationPaymentFailure = 1021,
        BookingFinalizationSuccess = 1022,
        BookingResponseProcessFailure = 1030,
        BookingResponseProcessStarted = 1031,
        BookingResponseProcessSuccess = 1032,
        AdministratorAuthorizationSuccess = 1100,
        AdministratorAuthorizationFailure = 1101,
        AgentAuthorizationSuccess = 1110,
        AgentAuthorizationFailure = 1111,
        CounterpartyStateAuthorizationSuccess = 1120,
        CounterpartyStateAuthorizationFailure = 1121,
        LocationNormalized = 1130,
        MultiProviderAvailabilitySearchStarted = 1140,
        ProviderAvailabilitySearchStarted = 1141,
        ProviderAvailabilitySearchSuccess = 1142,
        ProviderAvailabilitySearchFailure = 1143
    }
}