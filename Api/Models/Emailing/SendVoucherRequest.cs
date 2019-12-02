using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Emailing
{
    public readonly struct SendVoucherRequest
    {
        [JsonConstructor]
        public SendVoucherRequest(string email)
        {
            Email = email;
        }


        public string Email { get; }
    }
}