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
        CustomerInvitation = 11,
        RegularCustomerSuccsessfulRegistration = 12,
        AgencyManagement = 13
    }
}