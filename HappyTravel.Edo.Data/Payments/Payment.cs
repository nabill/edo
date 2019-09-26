using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Data.Payments
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int BookingId { get; set; }
        public Currencies Currency { get; set; }
        public string CustomerIp { get; set; }
        public string MaskedNumber { get; set; }
        public string CardHolderName { get; set; }
        public DateTime Created { get; set; }
        public PaymentStatuses Status { get; set; }
    }
}
