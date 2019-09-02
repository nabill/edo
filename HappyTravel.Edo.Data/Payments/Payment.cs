using System;
using System.Collections.Generic;
using System.Text;

namespace HappyTravel.Edo.Data.Payments
{
    public class Payment
    {
        public virtual int Id { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual int BookingId { get; set; }
        public virtual string Currency { get; set; }
        public virtual string CustomerIp { get; set; }
        public virtual string CardNumber { get; set; }
        public virtual string CardHolderName { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
    }
}
