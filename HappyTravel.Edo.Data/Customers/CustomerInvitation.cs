using System;

namespace HappyTravel.Edo.Data.Customers
{
    public class CustomerInvitation
    {
        public string CodeHash { get; set; }
        public string Data { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public bool IsAccepted { get; set; }
    }
}