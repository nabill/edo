using System;

namespace HappyTravel.Edo.Data.Booking
{
    public class CreditCardPaymentConfirmation
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        public int AdministratorId { get; set; }
        public DateTime ConfirmedAt { get; set; }
    }
}