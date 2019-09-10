using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Data.Payments
{
    public class Payment
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual int BookingId { get; set; }
        public virtual Currencies Currency { get; set; }
        public virtual string CustomerIp { get; set; }
        public virtual string CardNumber { get; set; }
        public virtual string CardHolderName { get; set; }
        public virtual DateTime Created { get; set; }
    }
}
