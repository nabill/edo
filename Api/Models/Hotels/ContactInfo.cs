using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct ContactInfo
    {
        [JsonConstructor]
        public ContactInfo(string email, string fax, string telephone)
        {
            Email = email;
            Fax = fax;
            Telephone = telephone;
        }


        public string Email { get; }
        public string Fax { get; }
        public string Telephone { get; }
    }
}
