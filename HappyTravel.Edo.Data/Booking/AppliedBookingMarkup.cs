using System;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Booking
{
    public class AppliedBookingMarkup
    {
        public string ReferenceCode { get; set; }
        public int PolicyId { get; set; }
        public decimal Amount { get; set; }
        public Currencies Currency { get; set; }
        public DateTime? Payed { get; set; }
    }
}