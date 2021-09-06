namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public static class EventTypes
    {
        public const string Authorised = "AUTHORISED";
        public const string Declined = "DECLINED";
        public const string ApmPaymentAccepted = "APM_PAYMENT_ACCEPTED";
        public const string AuthorisationFailed = "AUTHORISATION_FAILED";
        public const string FullAuthReversed = "FULL_AUTH_REVERSED";
        public const string FullAuthReversalFailed = "FULL_AUTH_REVERSAL_FAILED";
        public const string Captured = "CAPTURED";
        public const string CaptureFailed = "CAPTURE_FAILED";
        public const string CaptureVoided = "CAPTURE_VOIDED";
        public const string CaptureVoidFailed = "CAPTURE_VOID_FAILED";
        public const string CancellationRequested = "CANCELLATION_REQUESTED";
        public const string CancellationFailed = "CANCELLATION_FAILED";
        public const string Cancelled = "CANCELLED";
        public const string PartiallyCaptured = "PARTIALLY_CAPTURED";
        public const string PartialCaptureFailed = "PARTIAL_CAPTURE_FAILED";
        public const string Refunded = "REFUNDED";
        public const string RefundFailed = "REFUND_FAILED";
        public const string PartiallyRefunded = "PARTIALLY_REFUNDED";
        public const string RefundRequested = "REFUND_REQUESTED";
        public const string RefundRequestFailed = "REFUND_REQUEST_FAILED";
        public const string PartialRefundFailed = "PARTIAL_REFUND_FAILED";
        public const string PartialRefundRequestFailed = "PARTIAL_REFUND_REQUEST_FAILED";
        public const string PartialRefundRequested = "PARTIAL_REFUND_REQUESTED";
        public const string RefundVoided = "REFUND_VOIDED";
        public const string RefundVoidFailed = "REFUND_VOID_FAILED";
        public const string RefundVoidRequested = "REFUND_VOID_REQUESTED";
        public const string GatewayRiskPreAuthRejected = "GATEWAY_RISK_PRE_AUTH_REJECTED";
        public const string PreAuthFraudCheckRejected = "PRE_AUTH_FRAUD_CHECK_REJECTED";
        public const string PostAuthFraudCheckRejected = "POST_AUTH_FRAUD_CHECK_REJECTED";
        public const string PostAuthFraudCheckReview = "POST_AUTH_FRAUD_CHECK_REVIEW";
        public const string PostAuthFraudCheckAccepted = "POST_AUTH_FRAUD_CHECK_ACCEPTED";

    }
}