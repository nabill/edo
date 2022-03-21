using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings.Invoices
{
    public readonly struct BuyerInfo
    {
        [JsonConstructor]
        public BuyerInfo(string name, string address, string contactPhone, string email)
        {
            Address = address;
            ContactPhone = contactPhone;
            Email = email;
            Name = name;
        }


        public string Address { get; }
        public string ContactPhone { get; }
        public string Email { get; }
        public string Name { get; }
    }
}
