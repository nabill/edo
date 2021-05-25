namespace HappyTravel.Edo.Notifications.Enums
{
    public enum NotificationTypes
    {
        None = 0,
        // Booking
        BookingVoucher = 1,
        BookingInvoice = 2,
        DeadlineApproaching = 3,
        SuccessfulPaymentReceipt = 4,
        BookingDuePaymentDate = 5,
        BookingCancelled = 6,
        BookingFinalized = 7,
        BookingStatusChanged = 8,
        // Accounts
        CreditCardPaymentReceived = 9,
        AccountBalanceReplenished = 10,
        // Counterparty
        RegularCustomerInvitation = 11,
        ChildAgencyInvitation = 12,
        RegularCustomerSuccessfulRegistration = 13,
        ChildAgencySuccessfulRegistration = 14,
        AgencyManagement = 15,
        // Administrator
        MasterCustomerInvitation = 16,
        MasterCustomerSuccessfulRegistration = 17,
        BookingsAdministratorSummaryNotification = 18,
        BookingCancelledToReservations = 19,
        BookingFinalizedToReservations = 20,
        CreditCardPaymentReceivedAdministrator = 21
    }
}