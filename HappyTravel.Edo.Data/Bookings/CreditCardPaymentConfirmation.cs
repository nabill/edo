using System;

namespace HappyTravel.Edo.Data.Bookings
{
    public class CreditCardPaymentConfirmation
    {
        public int BookingId { get; set; }
        public int AdministratorId { get; set; }
        public DateTimeOffset ConfirmedAt { get; set; }
    }
}