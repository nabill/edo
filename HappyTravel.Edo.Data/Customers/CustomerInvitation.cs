using System;

namespace HappyTravel.Edo.Data.Customers
{
    public class CustomerInvitation
    {
        public string Code { get; set; }
        public string Data { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public bool IsAccepted { get; set; }
    }
}