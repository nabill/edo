using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentResponse
    {
        [JsonConstructor]
        public PaymentResponse(string secure3d)
        {
            Secure3d = secure3d;
        }

        public string Secure3d { get; }
    }
}
